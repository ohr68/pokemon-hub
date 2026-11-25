# Roadmap

| Phase | Status |
|---|---|
| 0. Foundation: modular structure, Users, Keycloak, Docker | Done |
| 1. Pokédex: species catalog and sync, trainers, starters, collection, ownership history, progress | Done |
| 2. Frontend integration: Keycloak sign-in, catalog list, Meus Pokémons | Done |
| 3. Trades | Done: trades between trainers, and moving Pokémon between saves through the hub |
| 4. Games platform | Done: registration, game API, files, rate limits, webhooks, turning games off, admin pages |
| 5. FireRed | Done: play in the browser, sync, mirror, save backups, transfers both ways |
| 6. Operations | Done: backups, reliable startup, problems view, local checks |
| 7. Trainer progression: XP and levels, trophies, badges, coins | Done |
| 8. Connected achievements: general, per-game, cross-game and specific | Done |
| 9. LeafGreen: a second connected game | Done |
| 10. Ruby: the first game from another save family | Done |
| 11. Sapphire: Ruby's pair | Done |
| 12. Emerald: the third save layout | Done; Hoenn badge positions wait on a first badge ([tech debt](tech-debt.md)) |
| 13. 3DS spike: can a 3DS game run in the browser | Stopped: a 3DS game doesn't fit in the browser emulator's memory |
| 14. Gen 7 save loader: Sun, Moon, Ultra Sun and Ultra Moon saves, read | Done; champion trophies wait on the games' counters |
| 15. Gen 6 save loader: X and Y saves, read | Paused before 15a, for Phase 16 |
| 16. 3DS sync app: a homebrew app that sends the console's saves to PokeHub | Scoped, not started |

More ideas, not yet in a phase, are in the [backlog](#backlog).

Tests for the Pokédex, Trades and Games modules, a shared integration-test base and CI are
still to do. Known gaps are tracked in [tech-debt.md](tech-debt.md).

## 3. Trades

Start simple, then grow with the games:

1. **Trades between trainers** (done): one Pokémon for one Pokémon, like a link trade in
   the main-series games. Trainers find each other by trainer code; one proposes, the other
   accepts or rejects, and the swap happens in one Pokédex transaction through
   `IPokedexApi`. Original trainers are kept, received species count in the Pokédex, and
   the eight plain trade evolutions (Kadabra, Machoke, Graveler, Haunter, Boldore, Gurdurr,
   Phantump, Pumpkaboo) evolve on arrival. Offers expire after 7 days.

   Room to grow before step 2: notifications when an offer arrives (integration events and
   SignalR), trade evolutions that need a held item, and moving the swap to an
   event-driven saga once games run as separate services.
2. **Trades between a trainer's saves across games** (done with phase 5): a trainer
   moves a Pokémon out of one game to the hub, then into another game's save, like
   Pokémon HOME. See [5. FireRed](#5-firered).

### Note: look at PKHeX before step 2

[PKHeX](https://github.com/kwsch/PKHeX) is an open-source save editor for the
main-series games. Before designing save-to-save transfers, study how it:

- models a Pokémon across generations (the PKM formats) and converts between them,
- checks a Pokémon is legal for a given game (legality analysis),
- reads and writes Pokémon in and out of a save.

It is a reference for how games store and validate Pokémon, which our own games' saves
will need too. PKHeX is GPL-3.0: reading it for ideas is fine, but referencing or copying
its code (for example the `PKHeX.Core` package) would require PokeHub to be GPL as well.

## 4. Games platform

Browser games connect to PokeHub, and Pokémon live in the hub or in a game save, like
Pokémon HOME.

**First slice (done):**

- Admins register a game (`POST api/games`, `games:manage`); PokeHub creates its Keycloak
  public client (`game-{slug}`, PKCE) and allows its origins through CORS right away.
- Players sign in from the game with "Sign in with PokeHub". The token's `azp` claim tells
  PokeHub which game is calling; the game API (`api/game/...`) only accepts registered,
  active games.
- Each trainer has one save per game, created on first use. Games read their save, catch
  Pokémon into it (with a catch key, so a repeated request never creates a duplicate) and
  release Pokémon from it.
- In pokemon-web, trainers move Pokémon between the hub and their game saves. Trades only
  use hub Pokémon. Every catch and move is recorded in the ownership history.
- `tools/game-sandbox` is a one-page stand-in for a game, and the smallest example of
  what a browser game needs to do.

**Added with FireRed (phase 5):** save file backups (`api/game/save/files`), mirroring a
game's save (`api/game/save/reconcile`), and transfers between the hub and the game's own
save (`api/game/save/deliveries`, `api/game/save/withdrawals`).

**Second slice (done):**

- **Admin pages** (pokemon-web, `/admin/games`, for `games:manage`): create a game with its
  icon; edit its name, description and rate limit; upload its cover, icon, screenshots and
  ROM (`api/games/{id}/files/{kind}`, stored in Postgres); turn it on or off; set its webhook.
- **Games list and play page** for trainers (`/play`, `/play/{slug}`): covers, icons and
  screenshots (public, cacheable image URLs); a game with a ROM runs in the emulator, the
  ROM downloaded once by signed-in players (`api/games/{slug}/rom`) and cached in the
  browser until a new one is uploaded. Games without a ROM open on their own site.
- **Turning a game off** (`api/games/{id}/turn-off`, `turn-on`): it leaves the list, its
  Keycloak client is disabled, and the game API and CORS stop accepting it. Players' Pokémon
  and saves stay.
- **Per-game rate limits**: each player gets the game's own number of game API requests per
  minute (600 by default), counted per game; over it, `429` with `Retry-After`.
- **Webhooks**: the Pokédex publishes `PokemonSaveChanged`, and the Games module sends it to
  the game as `pokemon.moved_in`, `moved_out`, `delivered`, `withdrawn`, `released` or
  `left_game` (plus `ping` for tests). Requests are signed (`PokeHub-Signature:
  t=…,v1=HMAC-SHA256 of "t.body"` with the game's secret), retried with growing waits for
  about an hour, and listed on the admin page.

**Room to grow:** signing in to non-FireRed games from pokemon-web (each would need its own
client session), and admin-chosen emulator cores for other consoles.

## 5. FireRed

Pokémon FireRed connected to PokeHub, as the first real game. It runs in pokemon-web's
Jogar page and signs in as its own game, `game-firered`.

- **5a. Play in the browser** (done): EmulatorJS (mGBA core) runs the game in its own page
  inside the app. At first the player picked their own ROM, kept only in their browser; now
  the admin uploads it once (phase 4, second slice) and players download it from PokeHub. A
  personal setup: PokeHub isn't meant to be deployed publicly with a commercial ROM. In-game
  saves are written to the browser every 15 seconds and when leaving the page.
- **5b. Sync to PokeHub** (done): pokemon-web reads the Generation III save itself (party and
  PC boxes, decrypted from the documented format) and records each Pokémon as a catch. Its
  personality value and original trainer id make the catch key, so syncing again never
  duplicates. Species numbers, growth rates (levels of box Pokémon) and nicknames come
  from the Pokédex catalog's Gen 3 data.
- **Mirror** (done): Pokémon synced before that are no longer in the game (released or
  traded away there) are listed after a sync; the player confirms before they're removed
  from PokeHub (`LeftGame` in the ownership history).
- **Save backups** (done): every sync stores the save file on PokeHub (the 20 newest per
  game; an unchanged save isn't stored twice). The player can restore any backup into the
  game; the current save is backed up first.
- **5c. Transfers both ways** (done), like Pokémon HOME:
  - Hub to FireRed: a Pokémon moved into the FireRed save in Meus Pokémons waits there until
    the Jogar page writes it into a free PC box slot. A Pokémon that was in a Gen 3 game
    before comes back exactly as it left; a new one gets the moves it would know at its
    level (FireRed/LeafGreen learnsets from PokéAPI), random IVs and nature, and the
    player as original trainer when they are, or its own trainer when it was traded.
  - FireRed to hub: the player picks PC box Pokémon to send; their exact bytes (pk3) are
    kept, and level, evolution and nickname changes from the game are applied.
  - A Pokémon inside a game can only leave it from the game, so it is never in both
    places. Both directions back the save up first, then restart the game from the
    changed save.

Tested against a real FireRed save: the game loads the changed save and writes delivered
Pokémon back unchanged when it saves.

**Open:** the summary screen of a delivered Pokémon (original trainer, met location) is
still to check in the game, and Deoxys has no FireRed learnset yet (PokéAPI lists it under
another form).

## 6. Operations

PokeHub stays private, so this phase is about not losing data and knowing when something
breaks, not about going to production. Health checks (`/health`), Serilog with Seq and a
global exception handler already exist.

Steps, in order:

- **6a. Backups and restore** (done): saves, Pokémon, coins and the ROM all live in one
  Docker volume, so backups now go to a folder outside Docker. A `pokehub.backup` service
  dumps the database daily and `tools/ops/backup.sh` also copies Keycloak, which has to stop
  for a few seconds. Restores load on the side and keep what they replace. See
  [tools/ops/README.md](../tools/ops/README.md).
- **6b. Reliable startup** (done): every service restarts on its own and has a health
  check, the API waits for healthy dependencies, and image versions are pinned. After a
  Docker restart the containers come up in any order, so the API also waits for the database
  before running migrations.
- **6c. Problems view** (done): the Sistema tab in admin shows service health, the age of
  the last backups, failed or stuck messages per module, recent failures, and webhooks that
  are retrying or gave up. It only reads (`GET system/status`, permission `system:read`).
- **6d. Local verify script** (done): `tools/ops/verify.sh` runs the backend build,
  architecture tests, frontend types, lint and vitest on both repos and prints one summary.
  There's no hosted CI.
- **Role granting** (done, added after the phase): the Usuários tab in admin makes or
  unmakes administrators (permission `users:roles`). Nobody can remove their own role, so
  one always remains, and each change is recorded as a `UserRoleChanged` event. If no
  administrator exists, `Users:FirstAdministratorEmail` sets up the first one at startup.

Left out on purpose: public deployment, HTTPS and production hardening (the instance hosts
a commercial ROM and must stay private), metrics dashboards and tracing, hosted CI, the full
module test suite, and the items in [tech-debt.md](tech-debt.md).

## 7. Trainer progression

Trainers grow by playing, across every game connected to PokeHub:

- **XP and trainer level** (done): a Progression module listens to the Pokédex's
  integration events and awards XP: choosing the starter 50, each catch in a game 10, a
  shiny 50 more, a species new to the trainer 25 more, each trade 20, each evolution 15
  (granted Pokémon earn nothing). Each award is an entry with a source key, so nothing pays
  twice. Levels: 100 XP from level 1 to 2, then 50 more each level, up to 100. Reaching a
  level publishes `TrainerLeveledUp` for trophies and coins to build on. Trainers see their
  level and latest XP in Meus Pokémons and the account menu (`api/progression/me`). Past
  activity was backfilled by replaying the Pokédex's ownership history as the same events
  (`api/pokedex/history/replay`, admins only; safe to repeat).
- **Trophies** (done): 18 trophies in bronze, silver and gold, defined in code
  (`Trophies.cs`): first steps, catches, species discovered, shinies, trades, evolutions and
  trainer level. Progression keeps a counter for each with every XP award and gives trophies
  as targets are reached, publishing `TrophyEarned` for coins to build on. At startup it
  also gives trainers any trophy they already qualify for, so new trophies reach them.
  Trainers see them, with progress toward the ones still locked, on their own page
  (`/trainer`, from the account menu), with their level and stats
  (`api/progression/me/trophies`).
- **Badges per game** (done): games report the badges a player has earned
  (`PUT api/game/save/badges`, every badge so far plus the game's total); the Games module
  publishes `GameBadgesReported`, and Progression records each new one per game, awards 100
  XP and counts it for three trophies (1, 8 and 16 badges). FireRed's sync reads the 8 Kanto
  badges from the save (flags 0x820 to 0x827, checked on a real save) and reports them. The
  trainer page shows a badge case per game (`api/progression/me/badges`). Badges are never
  removed, even if a save is restored to before one.
- **Coins** (done): an Economy module keeps each trainer's wallet and coin ledger (each
  source pays once). Coins come from catching a shiny (50) or a rare Pokémon (100:
  legendary or mythical, flags the catalog sync now fetches), badges (30), trophies (20,
  50, 100 by tier) and each new level (10 × the level). They're spent in the shop
  (`/shop`): admins put Pokémon up for sale (species, level, shiny, price, optional stock)
  at `/admin/shop`, and a purchase pays first, then has the Pokédex give the Pokémon to the
  buyer's hub (the purchase id becomes the Pokémon's, so it can't be given twice); if
  that fails, coins and stock go back. Bought Pokémon earn no XP or coins directly, but count as discovered species, which come
  from the Pokédex's history (however the trainer got them) and can earn species trophies. Past
  achievements were backfilled with the history replays (`api/pokedex/history/replay`,
  `api/progression/replay`). Items for the bag are on sale too: an admin picks from a
  safe catalog (balls, healing, vitamins, Rare Candy, evolution stones; no key items or
  TMs), and a purchase waits until the FireRed page writes it into the save, with a backup
  before and after.

## 8. Connected achievements

Achievements that follow a trainer across every game, so progress in one game counts toward
a larger journey. Trophies are general, per game, cross-game or hand-made for one game or the hub.

| Category | Examples |
|---|---|
| General | 500 catches across all games, 16 badges in total, trainer level |
| Per game (the same set for each game) | Catch 25, complete the regional Pokédex, evolve 10, find a shiny, earn all badges |
| Cross-game | Catch the same species in 2, 3 and 5 games, evolve it in each, complete 2 regional Pokédexes |
| Specific | FireRed: beat the Elite Four. General: a Pokémon that has visited 3 games |

- **8a. Reliable game data (done):** in-game evolutions are detected during sync, including
  skipped stages, and an older restored save doesn't count as one. Catch and evolution events
  carry their game, and past catches are read from the ownership history with no backfill.
- **8b. Achievement engine (done):** per-game and cross-game progress is worked out from game
  history and can award trophies of each kind. Trophies already earned stay, and their coins
  aren't paid twice. The new catalogs start empty and are filled by 8c and 8d.
- **8c. Per-game set (done):** one template of 11 trophies every game gets automatically,
  using the game Pokédex lists. Pokémon sent to a game count for its Pokédex goals, but not
  for catch goals. A game's Pokédex is found from its slug, so a new game's slug must be a
  list's slug or name just one version (`firered`).
- **8d. Cross-game set (done):** 11 trophies for playing several games, catching or evolving
  the same species across 2, 3 and 5 games, and completing 2 Pokédexes or badge sets. They pay
  double their tier. Only games with a known Pokédex count, so a sandbox game can't earn them.
- **8e. Specific achievements (done):** 7 hand-made goals. FireRed: beat the Elite Four, catch
  the legendary birds and Mewtwo, evolve the Eevee family. Hub: bring a Pokémon back from the
  hub, receive a trade evolution, take one Pokémon to 3 games.
- **8f. Trophy screen (done):** General, cross-game and per-game tabs, progress bars, and
  notifications that name the game.

Rules:

- "Every game" is a set of milestones (2, 3, 5 games). A literal "every game" would unlock on
  the first catch with one game connected, and become incomplete when another is added.
- Earned achievements are never revoked, even when a new game is added.
- A game only counts toward a species goal if that species exists in it.
- Rewards scale by tier, and cross-game achievements pay the most, so per-game sets don't
  flood the economy with coins.

## 9. LeafGreen

A second real game, so Pokémon can move between games and cross-game achievements can be
earned. Registration, ROM upload, sign-in clients, save reading and per-game trophies are
already generic, and each ROM has its own emulator save. The work is in the FireRed page code,
which assumes there is only one game.

Two risks to fix before LeafGreen goes live:

- **Wrong save restored:** the backup list is cached under one shared FireRed key, so LeafGreen
  could show FireRed's backups and restore the wrong one.
- **Reward farming:** both games use the same save format, so a FireRed save imported into
  LeafGreen passes validation. Its Pokémon would count as new catches, paying XP and coins and
  unlocking cross-game trophies.

Steps:

- **9a. Multi-game foundation:** sign-in, sync, transfers, backups and settings become per game.
  FireRed keeps its current data and connection.
- **9b. Save protection:** a Pokémon counts as a catch only if its origin data matches the game.
  Pokémon sent through the hub still arrive normally.
- **9c. LeafGreen live:** register the game, then enable play, sync, badges and transfers, with
  new Pokémon marked as LeafGreen's. The ROM is uploaded through the admin page.
- **9d. Shop items per game:** the player chooses which game receives a purchased item, and
  only that game's page puts it in the bag.
- **9e. Trophies and full test:** LeafGreen's specific trophies, then a real test of catching in
  both games, moving a Pokémon across, and the double-coin payout.

Rules:

- FireRed and LeafGreen count as two connected games, each with its own save, trophies and
  Pokédex progress.
- Completing the Pokédex in either game can use Pokémon sent from the other, which replaces
  trading between versions.
- The FireRed save is backed up before 9a, and play code isn't edited while the game is running.

## 10. Ruby

The first game from the other Gen 3 family (Ruby, Sapphire, Emerald). Registration, sign-in,
the Pokédex (Ruby & Sapphire, up to #386), per-game trophies, the Pokémon data in a save and
the PC boxes already work for it. The work is in the save code, which assumes FireRed and
LeafGreen's layout: Ruby keeps the party, money, bag, badge flags and game stats elsewhere,
doesn't encrypt money and items, and writes game code 0 instead of 1.

Two risks:

- **A damaged save:** Ruby's positions come from public documentation, not from a real save.
  A wrong one would corrupt the save on the first write, so Ruby is read-only until a real
  save confirms them, and every write is backed up first as before.
- **Items Ruby doesn't have:** some items exist only in FireRed and LeafGreen. None of them is
  in the shop's catalog today, so every item can go to Ruby; adding one would need a per-game
  check.

Steps:

- **10a. Save layout per game family:** the reader and the bag writer take the FireRed/LeafGreen
  or the Ruby layout, chosen by the game. A save from the other family is refused. Tests keep
  FireRed's behaviour as it is.
- **10b. Ruby live, read-only:** register Ruby (US code AXVE) and upload its ROM, then play to a
  first save and sync. The party, Hoenn badges, money and stats are checked against what the
  game shows before anything writes to the save.
- **10c. Writing into Ruby:** Pokémon from the hub arrive marked as Ruby's and met in Hoenn,
  Pokémon leave for the hub, and shop items go into Ruby's bag.
- **10d. Trophies and full test:** Ruby's specific trophies (champion, Groudon, the Hoenn
  exclusives), then a real test of a cross-game catch and a move between Kanto and Hoenn.

Rules:

- Ruby is a connected game like FireRed and LeafGreen, with its own save, trophies and Pokédex.
- Sapphire later needs only its origin number and ROM code. Emerald needs a third layout.

## 11. Sapphire

Ruby's pair, on the layout Phase 10 confirmed. The save, the PC, the bag, the Pokédex
(Ruby & Sapphire) and the trophy engine already work for it; Sapphire only differs in its
origin number (1), its ROM code (AXP) and its version exclusives.

Steps:

- **11a. Sapphire live:** list it with its ROM code, origin and Hoenn met location, register it
  and upload its ROM (US code AXPE), then play to a first save and check it as Ruby was.
- **11b. Trophies and test:** Sapphire's specific trophies (champion, Kyogre, Rayquaza, the
  Regis, and its exclusives: Lotad, Sableye, Seviper, Lunatone), then a Pokémon moved between
  Ruby and Sapphire, which replaces trading between the two cartridges.

Done: a first save read as Ruby's was (team, money, play time, Pokédex, counters), and a
Wurmple caught in Ruby moved through the hub into Sapphire's save. Sapphire shares Ruby's badge
positions, so the badge check in tech debt covers both.

Rules:

- Ruby and Sapphire count as two connected games, like FireRed and LeafGreen.
- A Ruby save loaded into Sapphire is refused as a catch source by the origin check (9b), as a
  FireRed one is in LeafGreen.

## 12. Emerald

The third Gen 3 save layout. Emerald keeps the party where Ruby does, but moves the flags,
the game counters and the bag pockets, and, like FireRed, XORs money, counters and item
quantities with a security key. That key sits where the other games write their game code,
so the game code can only hint at Emerald: a code other than 0 or 1 means Emerald, but a key
that happens to be 0 or 1 doesn't say so.

Two risks:

- **Telling Emerald apart:** a key that happens to be 0 or 1 would read as Ruby or FireRed.
  The family must come from the game the save is for, with the game code only as a check.
- **A damaged save:** as with Ruby, its positions come from documentation, so Emerald is
  read-only until a real save confirms them, and every write is backed up first.

Steps:

- **12a. Emerald layout:** the reader, stats and bag take Emerald's positions and key, with the
  family chosen by the game. Tests keep the other families as they are.
- **12b. Emerald live, read-only:** register it (US code BPEE, origin 3), play to a first save
  and check party, money, stats and badges against the game.
- **12c. Writing into Emerald:** Pokémon transfers and shop items, once a real save confirms the
  PC and the bag.
- **12d. Trophies and test:** Emerald's specific trophies (champion, Rayquaza, the Battle
  Frontier, its legendaries), then a move between Emerald and the other games.

Done: a first save read its team, money, play time, Pokédex and counters through the key, which
the game changes between saves. A shop Pikachu went into the PC and out to the hub, and 10 shop
Poké Balls stayed in the bag through the game's own save. Its trophies are the champion,
Rayquaza, the Regis, and Groudon with Kyogre. The Battle Frontier one waits on PokeHub reading a
Battle Frontier stat; the badge flags wait on a first badge.

## 13. 3DS spike

A short test of whether a 3DS Pokémon game can run on PokeHub at all, before any 3DS phase is
planned. EmulatorJS has a 3DS core, Azahar (`EJS_core = '3ds'`), which needs
`EJS_threads = true`. Everything else about it is unconfirmed, so the spike answers four
questions in order, and stops at the first one that fails.

Steps:

- **13a. Cross-origin isolation:** threads need SharedArrayBuffer, so the app and the emulator
  frame need COOP and COEP headers (`credentialless` if `require-corp` blocks too much). Check
  that sign-in, the game sign-ins, PokeAPI sprites, uploaded game icons and the EmulatorJS CDN
  all still load, in the dev server and in the Docker build.
- **13b. EmulatorJS upgrade:** move from 4.2.3 to the first version with the 3DS core. FireRed,
  LeafGreen, Ruby and Sapphire must still start, save, sync, restore and take transfers, with
  the save fixes in `public/emulator/index.html` kept.
- **13c. A game running:** one game (X or Y, the lightest likely candidates) on a 3DS page
  behind a flag. Note the frame rate in a battle and on the overworld, the load time, and what
  the core needs: a decrypted dump, keys from a console, or system files.
- **13d. The save out of the frame:** where Azahar keeps the game's save (a file on its virtual
  SD card, not the GBA save memory) and whether the frame can read it and write it back, as
  sync and restore need.

Rules:

- The spike ships nothing to players: the 3DS page stays behind a flag, and if it fails, the
  headers and the upgrade are kept only if they cost the GBA games nothing.
- No 3DS ROM, key, system file or save is committed, as with the GBA ROMs.
- Its result is a written decision here: a 3DS phase scoped (with the Gen 6 Pokémon data, the
  pk6 format and moving Pokémon across generations), DS (Gen 4/5) first instead, or neither.

Result: stopped at 13c, and 3DS games aren't emulated.

- **13a worked:** with COOP and COEP on the dev server, the app and the emulator frame were
  isolated, and sign-in, PokéAPI, its sprites and FireRed all still worked. Game images needed
  a `Cross-Origin-Resource-Policy` header from the API.
- **13b changed:** the 3DS core only exists in EmulatorJS 4.3.0-pre, a pre-release, so the
  test ran it in a frame of its own and left the GBA games on 4.2.3.
- **13c failed:** the core downloaded and unpacked, but EmulatorJS reads the whole game into
  one block of memory and copies it into 32-bit WebAssembly, which holds at most 4 GB. A
  Pokémon Sun dump is 4 GB, so it failed before the core ran it. Sun, Moon and the Ultra games
  can't fit; X and Y might once trimmed, but a fix would mean the emulator reading the game
  from disk as it goes, which EmulatorJS doesn't do.
- **Decision:** the spike's code was removed. 3DS games are played on a console or a desktop
  emulator, and PokeHub reads their saves instead (Phase 14).

## 14. Gen 7 save loader

3DS games are played outside PokeHub, on a console or in the Azahar desktop app. The player
uploads the game's save, and PokeHub reads it like a GBA sync: new catches, levels, money, play
time and the Pokédex count towards the trainer, trophies and coins. The save itself is never
changed: this first version only reads.

Gen 7 first: Sun, Moon, Ultra Sun and Ultra Moon share the pk7 format and close save layouts,
and PokeHub's Pokédex already has their dexes (#802 and #807). X, Y, Omega Ruby and Alpha
Sapphire (Gen 6) follow in a later phase.

Where the save comes from: Checkpoint or JKSM on a console with homebrew, or Azahar's save
folder on a computer. It's the game's `main` file, about 440 KB (Sun/Moon `0x6BE00` bytes,
Ultra Sun/Ultra Moon `0x6CC00`).

Steps:

- **14a. Gen 7 save reader:** in pokemon-web, next to `lib/gen3`. Tell Sun/Moon from the Ultra
  games by size, check the blocks' checksums, and read the trainer (name, ID, secret ID), the
  party and the 32 PC boxes (pk7: decrypted with the encryption constant, checked by its
  checksum), money, play time and the Pokédex. Tests build saves in code; no real save is
  committed.
- **14b. Save upload page:** a registered Gen 7 game has no emulator. Its Jogar page offers
  "Carregar save" instead, reads the file in the browser and syncs it through the game's own
  connection, with the same catch, level and stats calls the GBA sync uses, and keeps a backup.
  Moving Pokémon to the hub, shop items, restoring and the emulator's tools are hidden for it.
- **14c. Live test and trophies:** register one game (Sun or Moon, say), upload a real save and
  check its numbers against the game; then its specific trophies (champion, the Tapus, Solgaleo
  or Lunala, the Ultra Beasts).

Rules:

- A Pokémon counts as caught in the game when its origin is that game (Sun 30, Moon 31,
  Ultra Sun 32, Ultra Moon 33). One brought from another game through Pokémon Bank or the
  Poké Transporter is from another game, so it pays no catch rewards, as with a Ruby save in
  Sapphire.
- Shiny in Gen 7 is the trainer and secret IDs XORed with the personality value's halves,
  below 16 (Gen 3 uses below 8).
- Alola has trials instead of gym badges, so these games report no badges.
- Read-only: Pokémon stay in the save and can't move to the hub or between games yet. Writing
  into a save (and converting Pokémon between generations) is a later phase.

Done: a real Moon save read on the first upload. Its checksums passed, its five Pokémon came in
with the moves their levels give, and money, play time and the Pokédex matched the game. Box
levels come from a new endpoint with every species' growth rate (`pokedex/species/growth`).
Sun and Moon have their Tapus, Solgaleo or Lunala and Ultra Beasts trophies.

Left for later:

- **Champion trophies:** they need the games' counters (the Hall of Fame), which this version
  doesn't read; it reports money, play time and the Pokédex only.
- **Hidden abilities:** PokeHub keeps an ability slot of 0 or 1, so a hidden ability is kept
  as the first slot.
- **Ultra Sun and Ultra Moon:** read like Sun and Moon by their size, but no real save has
  confirmed them yet.

## 15. Gen 6 save loader

X and Y on Phase 14's path: played on a console or in Azahar, with the save uploaded on the
game's page and read, never changed. PokeHub's Pokédex already has their dex (X & Y, #721).
Omega Ruby and Alpha Sapphire can follow the same way, as Ultra Sun and Ultra Moon did.

What carries over from Phase 14: the upload page, the sync (backup, catches, levels, battle
data, money, play time, Pokédex, Pokémon that left the save), the growth rates endpoint, and
the Pokémon themselves. Gen 6's pk6 has pk7's size, encryption, checksum and nearly all of its
fields, so the decoder is shared.

What's new is the save around them. It's the game's `main` file again (X and Y `0x65600`
bytes), a row of blocks with a table at the end, but with:

- **Its own blocks:** other ids and lengths for the trainer, the party, the Pokédex, money and
  play time, found through the table as in Gen 7 and checked by their known lengths.
- **31 boxes** instead of 32 (`0x34AD0` bytes).
- **Another checksum:** CRC-16-CCITT (`0x1021`, starting at `0xFFFF`) instead of Gen 7's.
- **Its own Pokédex layout.**

Steps:

- **15a. Gen 6 save reader:** the block table, checksums, trainer, party, boxes, money, play
  time and Pokédex for X and Y, with the pk7 decoder shared as a Gen 6/7 one. Tests build saves
  in code. The positions come from documentation; the player's real X save confirms them, by
  uploading it once 15b is in.
- **15b. X and Y on the upload page:** listed with their origin numbers (X 24, Y 25), so their
  pages take a save; then the real X save is checked against the game (money, play time,
  Pokédex, the Pokémon and their moves).
- **15c. Trophies:** X's and Y's specific trophies (Xerneas or Yveltal, Mewtwo, the legendary
  birds, Zygarde), and the champion one if the Hall of Fame can be read.

Rules:

- As in Gen 7: a Pokémon counts as caught in the game only when its origin is that game; one
  brought through the Poké Transporter or Pokémon Bank pays no catch rewards.
- Shiny below 16, as in Gen 7.
- Kalos has gyms: X and Y report their 8 badges, read from the save's flags, which a first badge
  confirms, as with Ruby's.
- Read-only, as in Phase 14.

Paused before 15a, for Phase 16: uploading saves by hand is the step to remove first.

## 16. 3DS sync app

A 3DS never shows a save as a file, so today the player copies it out with Checkpoint or JKSM
and uploads it. A homebrew app for consoles with custom firmware, "PokeHub Sync", sends the
saves of the Pokémon games on the console straight to PokeHub instead.

How it fits:

- **Signing in:** by code, with the OAuth device flow, which Keycloak supports per client. The
  app shows a short code, the player approves it on their phone or computer, and the app gets a
  token for that game's own client (`game-pokemon-moon`, say), as the game's page does. No
  password is typed on the console. The refresh token stays on the console's SD card.
- **Reading:** each Pokémon game on the console (cartridge or digital), through the same
  system calls Checkpoint uses. The app only reads saves.
- **Sending:** to the save backups PokeHub already keeps (`game/save/files`), labelled as from
  the 3DS. The app doesn't read Pokémon itself.
- **Syncing:** the game's page syncs the newest backup it hasn't read yet when it opens, with
  the reader it already has. A backend reader (the Gen 7 one in C#) would make the sync happen
  as soon as the console sends, with no page open; that's a later step.

Steps:

- **16a. Sign-in by code:** turn the device flow on for game clients, in `GameClientService` for
  new games and once for the existing ones, then sign in by code from a terminal and upload a
  save with that token, to prove the path before any console code.
- **16b. App skeleton:** a new repository, built with devkitPro (devkitARM, libctru, citro2d).
  It lists the Pokémon games on the console by title id, reads one game's save and shows its
  size. It runs in Azahar first, then on a real console.
- **16c. Sign-in and upload from the app:** the code screen, the token kept on the SD card, and
  the save sent to the game's backups. The console's own HTTPS is outdated, so the app brings
  its own (libcurl with mbedTLS, as homebrew usually does); PokeHub on the same network works
  over plain HTTP meanwhile.
- **16d. The page picks it up:** a game's page notices a 3DS backup newer than its last sync
  and syncs it, showing where it came from.

Rules:

- Only games PokeHub reads: Sun and Moon now, X and Y once Phase 15 resumes. The app lists the
  others as not supported yet.
- Read-only on the console: the app never writes a save. Restoring or writing Pokémon into a
  3DS save is a later phase.
- No save, token or key is committed, in this repository or the app's.
- Custom firmware is the player's own setup; the app's README says what it needs and doesn't
  ship anything from Nintendo.

Open before 16b: where the app's repository lives, and whether Azahar's emulated network lets
the app reach PokeHub, or only a real console can test 16c.

## Backlog

- **Unchecked flows:** built but never checked by hand in the browser, so they may hide layout
  or touch bugs: dragging the shop carousel by touch, the new trade flow's layout, and making a
  real offer through the offer forms. Check each at phone and desktop sizes and fix what's
  wrong.
- **Fluid save sync** (done for the browser; what's left is in
  [tech-debt.md](tech-debt.md#save-sync-isnt-fluid)), in four steps:
  1. **Safe transfers** (done): PokeHub records a transfer before the game's save changes, so
     the browser notes it first. One cut short by a failed restore or a closed tab is settled
     against what the save really holds: a withdrawn Pokémon still there goes back inside the
     game, and a delivered one that's missing returns to the queue. The save is backed up after
     every transfer, and automatic sync can't start as a transfer begins.
  2. **Instant sync** (done): each in-game save is noticed and synced within about 20 seconds,
     on by default with an off switch. Every read through the emulator rewrites the whole
     128 KB save to IndexedDB, even unchanged, so changes are detected on the 15-second flush
     the page already does instead of by polling faster.
  3. **One save panel** (done): a single status (synced, syncing, failed, no save yet) with the
     last sync time, and tabs for transfers, items, backups and devices. Tabs stay
     mounted, so a half-picked PC box or a receipt survives switching between them.
  4. **Fewer disruptive restarts** (done): pending Pokémon and items go in together, with one
     restart, when the game starts and there's no progress to lose. Mid-game they wait until
     the player applies them, with a warning that says how long ago the last save was.

  A game also runs in only one tab at a time (Web Locks): all tabs share one save in IndexedDB,
  so a second running copy would overwrite it. A waiting tab takes over when the first closes.

  Left out for now: writing into the running game's memory to avoid restarts (risky, needs its
  own spike), automating the 3DS routine, and detecting old save states.
- **Automatic sync** (done for FireRed): while the game is open, it syncs after each in-game
  save. Turning it off is kept per account in the browser, so it doesn't follow the trainer to
  another device.
- **Battle data per Pokémon** (done for FireRed): each sync stores what the save holds about
  a Pokémon (level, nature, moves and PP, IVs, EVs, friendship, held item and, for the party,
  current stats), per game. Each owned Pokémon has its own page (`/my-pokemon/:id`) with its
  evolution chain and its status in every game it has been in. It keeps the latest report per
  game: a Gen 3 save has no battle history per Pokémon, so growth over time would need keeping
  past syncs.
- **Stats sync per game** (done for FireRed): each sync stores the game's play time, money,
  Pokédex and counters (steps, battles, captures...), shown per game on the trainer's page.
  Counters are kept by name, so other games can report their own.
- **Custom SVG type icons** (done): all 18 types have original icons drawn on Lucide's grid
  (24px, 2px rounded strokes) and built with its `createLucideIcon`, so badges and card
  patterns use them like any other Lucide icon.
- **Saves between consoles and emulators** (done for files): every way in or out of a
  GBA device ends in the same raw save file (.sav; .srm in RetroArch): emulators (mGBA,
  VisualBoyAdvance, myBoy, Pizza Boy, Delta) read and write it, flash carts keep it on
  their SD card, and USB cartridge readers (GBxCart RW, GB Operator, with FlashGBX) dump and
  write it. So the Jogar page imports one (checked to be FireRed/LeafGreen by the game code
  the save records, trimmed to 128 KB when an emulator added bytes, previewed, stored as a
  labeled backup and optionally loaded into the game) and exports the current save or any
  backup, with a short guide per device. 3DS Virtual Console: while the game runs, its
  save sits in a NAND partition behind a 512-byte header with a CMAC signed with the
  console's keys, which a browser can't produce; GodMode9 dumps and injects it
  (`agbsave.bin`), and the import strips that header when present. Still to study: talking
  to a cartridge reader straight from the browser (Web Serial), which would skip the
  desktop tool.
