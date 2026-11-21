namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>
/// A trophy a trainer earns once <see cref="Stat"/> reaches <see cref="Target"/>. Trophies are
/// defined in code (<see cref="Trophies"/>); only which ones a trainer earned is stored.
/// </summary>
/// <param name="Code">Stable id, stored with the trainer: never rename one.</param>
public sealed record Trophy(string Code, string Name, string Description, TrophyTier Tier, TrainerStat Stat, int Target);

public static class Trophies
{
    public static readonly IReadOnlyList<Trophy> All =
    [
        new("first-steps", "Primeiros passos", "Escolha seu Pokémon inicial.", TrophyTier.Bronze, TrainerStat.StartersChosen, 1),

        new("first-catch", "Primeira captura", "Capture um Pokémon em um jogo.", TrophyTier.Bronze, TrainerStat.Catches, 1),
        new("catcher-25", "Caçador", "Capture 25 Pokémon.", TrophyTier.Bronze, TrainerStat.Catches, 25),
        new("catcher-100", "Caçador experiente", "Capture 100 Pokémon.", TrophyTier.Silver, TrainerStat.Catches, 100),
        new("catcher-500", "Mestre caçador", "Capture 500 Pokémon.", TrophyTier.Gold, TrainerStat.Catches, 500),

        new("species-10", "Pesquisador", "Descubra 10 espécies.", TrophyTier.Bronze, TrainerStat.SpeciesDiscovered, 10),
        new("species-50", "Pesquisador dedicado", "Descubra 50 espécies.", TrophyTier.Silver, TrainerStat.SpeciesDiscovered, 50),
        new("species-151", "Pokédex de Kanto", "Descubra 151 espécies.", TrophyTier.Gold, TrainerStat.SpeciesDiscovered, 151),

        new("first-shiny", "Brilho raro", "Capture um Pokémon shiny.", TrophyTier.Silver, TrainerStat.ShinyCatches, 1),
        new("shiny-5", "Caçador de shinies", "Capture 5 Pokémon shiny.", TrophyTier.Gold, TrainerStat.ShinyCatches, 5),

        new("first-trade", "Primeira troca", "Faça uma troca com outro treinador.", TrophyTier.Bronze, TrainerStat.Trades, 1),
        new("trader-10", "Negociante", "Faça 10 trocas.", TrophyTier.Silver, TrainerStat.Trades, 10),
        new("trader-50", "Mestre das trocas", "Faça 50 trocas.", TrophyTier.Gold, TrainerStat.Trades, 50),

        new("first-evolution", "Evolução", "Evolua um Pokémon.", TrophyTier.Bronze, TrainerStat.Evolutions, 1),
        new("evolver-10", "Treinador dedicado", "Evolua 10 Pokémon.", TrophyTier.Silver, TrainerStat.Evolutions, 10),

        new("level-5", "Nível 5", "Chegue ao nível 5 de treinador.", TrophyTier.Bronze, TrainerStat.Level, 5),
        new("level-10", "Nível 10", "Chegue ao nível 10 de treinador.", TrophyTier.Silver, TrainerStat.Level, 10),
        new("level-25", "Nível 25", "Chegue ao nível 25 de treinador.", TrophyTier.Gold, TrainerStat.Level, 25),
    ];
}
