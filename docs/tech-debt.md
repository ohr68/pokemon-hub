# Technical debt

Known gaps we chose to leave for later. Each entry says what is missing, why it matters
and the direction for fixing it.

## Trainers for accounts created before the Pokédex module

**What's missing:** users registered before the Pokédex module existed never got a
trainer, because their `UserRegistered` events were dropped by the old outbox bug.
They see "Preparing your trainer card..." forever.

**Direction:** a one-off backfill that creates a trainer for every user without one,
safe to run more than once.

## Save sync isn't fluid

**What's missing:** moving data between the game and the hub takes too many manual steps.

- Sync only sees what was last saved inside the game, so the player has to remember to save
  first.
- Changing the save mid-game still restarts it from the last save. The player chooses when and
  is told how old that save is, but progress since then is dropped.
- Save states aren't part of the save file, so loading an older one can hide a delivery.
- On the 3DS, each trip is a manual GodMode9 and FTP routine.

**Why it matters:** it's the main loop of the FireRed integration. Each extra step is a
chance to lose progress or think a transfer failed, as with a delivered Squirtle that was
in Box 1 and looked missing.

**Direction:**
- Study whether a transfer can avoid the restart by writing into the running game's memory.
  It's riskier than editing the save file, so it needs its own spike.
