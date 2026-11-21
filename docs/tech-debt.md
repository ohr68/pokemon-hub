# Technical debt

Known gaps we chose to leave for later. Each entry says what is missing, why it matters
and the direction for fixing it.

## Role granting

**What's missing:** there is no way to give or take a role (Member, Administrator)
through the API or the app. Making someone an administrator means editing
`users.user_roles` in the database by hand.

**Why it matters:** admin-only features (`species:sync`, `pokemon:grant`) can't be
handed out safely, and role changes leave no trace.

**Direction:**
- Endpoints in the Users module to list roles and grant/revoke them, protected by a new
  `users:roles` permission that only administrators have.
- A domain event when roles change, so it is auditable.
- A way to create the first administrator (seeded from configuration on startup, or a
  one-off CLI command), since nobody can grant roles before an admin exists.
- Permissions are read per request from the database today; if they get cached later,
  invalidate the cache on role changes.

## Trainers for accounts created before the Pokédex module

**What's missing:** users registered before the Pokédex module existed never got a
trainer, because their `UserRegistered` events were dropped by the old outbox bug.
They see "Preparing your trainer card..." forever.

**Direction:** a one-off backfill that creates a trainer for every user without one,
safe to run more than once.

## Save sync isn't fluid

**What's missing:** moving data between the game and the hub takes too many manual steps.

- Sync only sees what was last saved inside the game, so the player has to remember to save
  first, and automatic sync is off until they choose an interval.
- Sending or withdrawing a Pokémon restarts the game from its last save, which drops any
  progress since then and takes the player back to the title screen.
- Sync, transfers, import/export and backups are separate panels with their own buttons.
- Save states aren't part of the save file, so loading an older one can hide a delivery.
- On the 3DS, each trip is a manual GodMode9 and FTP routine.

**Why it matters:** it's the main loop of the FireRed integration. Each extra step is a
chance to lose progress or think a transfer failed, as with a delivered Squirtle that was
in Box 1 and looked missing.

**Direction:**
- Notice when the game saves and sync right away, instead of waiting for the automatic
  sync's next turn.
- Back up the save right after a transfer changes it. Today the backup is taken before the
  change, so the delivered save is only on the server after the next sync.
- Show one sync state on the Jogar page: up to date, syncing, unsaved or failed, with the
  last sync time.
- Queue transfers and apply them at a safe moment, so the player chooses when the game
  restarts, and warn before a restart if the game hasn't been saved recently.
- Bring sync, transfers, import/export and backups into one save panel.
- Study whether a transfer can avoid the restart by writing into the running game's memory.
  It's riskier than editing the save file, so it needs its own spike.
