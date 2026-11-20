# Roadmap

| Phase | Status |
|---|---|
| 0. Foundation: modular structure, Users, Keycloak, Docker | Done |
| 1. Pokédex: species catalog and sync, trainers, starters, collection, ownership history, progress | Done |
| 2. Frontend integration: Keycloak sign-in, catalog list, Meus Pokémons | Done |
| 3. Trades | Done: trades between trainers, and moving Pokémon between saves through the hub |
| 4. Games platform | Done: registration, game API, files, rate limits, webhooks, turning games off, admin pages |
| 5. FireRed | Done: play your own ROM, sync, mirror, save backups, transfers both ways |
| 6. Operations | Planned |

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
