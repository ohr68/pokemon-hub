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

## Hoenn badge positions are unconfirmed

**What's missing:** Ruby, Sapphire and Emerald read their saves from positions taken from public
documentation. Real saves confirmed the party, money, play time, Pokédex, game counters, PC and
bag, but not the badge flags: no test save has a badge yet, and the first gym is a long way in.
Ruby and Sapphire share `RS_LAYOUT`'s badge flags; Emerald has its own (`E_LAYOUT`, flags at
`0x1270`, first badge flag `0x867`).

**Why it matters:** a wrong position would show those games' badges wrongly on the trainer page
and in their badge trophies. Badges are only read, never written, so a save can't be damaged.

**Direction:**
- After the first badge (Roxanne's Stone Badge) in Ruby or Sapphire, and again in Emerald, sync
  and check the trainer page shows it.
- If it doesn't, find the flags in that save and correct the layout in pokemon-web's
  `src/lib/gen3/layout.ts`.
