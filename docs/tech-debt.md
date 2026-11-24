# Technical debt

Known gaps we chose to leave for later. Each entry says what is missing, why it matters
and the direction for fixing it.

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

## Ruby's badge positions are unconfirmed

**What's missing:** Ruby reads its save from positions taken from public documentation. A real
save confirmed the party, money, play time, Pokédex and game counters, but not the badge flags:
the test save had no badges yet, and the first gym is a long way in.

**Why it matters:** a wrong position would show Ruby's badges wrongly on the trainer page and
in its badge trophies. Phase 10c, writing into Ruby's save, waits on a fully confirmed layout,
since a wrong position there would damage the save.

**Direction:**
- After the first badge (Roxanne's Stone Badge), sync Ruby and check the trainer page shows it.
- If it doesn't, find the flags in that save and correct `RS_LAYOUT` in
  pokemon-web's `src/lib/gen3/layout.ts`, then start 10c.
