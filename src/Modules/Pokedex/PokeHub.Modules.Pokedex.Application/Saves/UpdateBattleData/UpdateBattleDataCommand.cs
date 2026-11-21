using System.Text.Json;
using FluentValidation;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Pokemon;
using PokeHub.Modules.Pokedex.Domain.Saves;

namespace PokeHub.Modules.Pokedex.Application.Saves.UpdateBattleData;

public sealed record UpdateBattleDataCommand(
    Guid TrainerId,
    Guid GameId,
    IReadOnlyCollection<BattleReport> Reports) : ICommand;

public sealed record BattleReport(string CatchKey, BattleData Data);

public sealed record BattleData(
    int Nature,
    int AbilitySlot,
    int HeldItemId,
    int Friendship,
    IReadOnlyList<BattleMove> Moves,
    IReadOnlyList<int> Ivs,
    IReadOnlyList<int> Evs,
    BattleStats? Stats);

public sealed record BattleMove(int Id, int Pp, int PpUps);

public sealed record BattleStats(int CurrentHp, int MaxHp, int Attack, int Defense, int Speed, int SpecialAttack, int SpecialDefense);

internal sealed class UpdateBattleDataCommandValidator : AbstractValidator<UpdateBattleDataCommand>
{
    private const int MaxReports = 500;
    private const int StatCount = 6;

    public UpdateBattleDataCommandValidator()
    {
        RuleFor(c => c.Reports).NotNull().Must(r => r.Count <= MaxReports);

        RuleForEach(c => c.Reports).ChildRules(report =>
        {
            report.RuleFor(r => r.CatchKey).NotEmpty().MaximumLength(100);
            report.RuleFor(r => r.Data).NotNull();
            report.RuleFor(r => r.Data.Nature).InclusiveBetween(0, 24);
            report.RuleFor(r => r.Data.AbilitySlot).InclusiveBetween(0, 1);
            report.RuleFor(r => r.Data.HeldItemId).InclusiveBetween(0, ushort.MaxValue);
            report.RuleFor(r => r.Data.Friendship).InclusiveBetween(0, 255);
            report.RuleFor(r => r.Data.Moves).NotNull().Must(m => m.Count <= 4);
            report.RuleForEach(r => r.Data.Moves).ChildRules(move =>
            {
                move.RuleFor(m => m.Id).InclusiveBetween(1, ushort.MaxValue);
                move.RuleFor(m => m.Pp).InclusiveBetween(0, 255);
                move.RuleFor(m => m.PpUps).InclusiveBetween(0, 3);
            });
            report.RuleFor(r => r.Data.Ivs).Must(v => v.Count == StatCount && v.All(x => x is >= 0 and <= 31));
            report.RuleFor(r => r.Data.Evs).Must(v => v.Count == StatCount && v.All(x => x is >= 0 and <= 255));
        });
    }
}

internal sealed class UpdateBattleDataCommandHandler(
    ISaveRepository saveRepository,
    IOwnedPokemonRepository pokemonRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBattleDataCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result> Handle(UpdateBattleDataCommand request, CancellationToken cancellationToken)
    {
        Save? save = await saveRepository.GetForGameAsync(request.TrainerId, request.GameId, cancellationToken);

        if (save is null)
        {
            return Result.Success();
        }

        var inGame = (await pokemonRepository.GetInsideGameAsync(save.Id, request.GameId, cancellationToken))
            .ToDictionary(p => p.CatchKey!, StringComparer.Ordinal);

        DateTime utcNow = dateTimeProvider.UtcNow;

        foreach (BattleReport report in request.Reports)
        {
            if (inGame.TryGetValue(report.CatchKey, out OwnedPokemon? pokemon))
            {
                pokemon.UpdateBattleData(JsonSerializer.Serialize(report.Data, JsonOptions), utcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
