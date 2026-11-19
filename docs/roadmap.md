# Roadmap

| Phase | Status |
|---|---|
| 0. Foundation: modular structure, Users, Keycloak, Docker | Done |
| 1. Pokédex: species catalog and sync, trainers, starters, collection, ownership history, progress | Done |
| 2. Frontend integration: Keycloak sign-in, catalog list, Meus Pokémons | Done |
| 3. Trades | In progress |
| 4. Games platform | Planned |
| 5. First original games | Planned |
| 6. Operations | Planned |

Tests for the Pokédex and Trades modules, a shared integration-test base and CI are
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
2. **Trades between a trainer's saves across games** (with phases 4–5): once games keep
   their own saves, a trainer can move Pokémon between their saves in different games
   through PokeHub.

### Note: look at PKHeX before step 2

[PKHeX](https://github.com/kwsch/PKHeX) is an open-source save editor for the
main-series games. Before designing save-to-save transfers, study how it:

- models a Pokémon across generations (the PKM formats) and converts between them,
- checks a Pokémon is legal for a given game (legality analysis),
- reads and writes Pokémon in and out of a save.

It is a reference for how games store and validate Pokémon, which our own games' saves
will need too. PKHeX is GPL-3.0: reading it for ideas is fine, but referencing or copying
its code (for example the `PKHeX.Core` package) would require PokeHub to be GPL as well.
