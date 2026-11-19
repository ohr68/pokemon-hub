using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Data;
using PokeHub.Modules.Pokedex.Domain.Trainers;

namespace PokeHub.Modules.Pokedex.Application.Trainers.CreateTrainer;

internal sealed class CreateTrainerCommandHandler(ITrainerRepository trainerRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTrainerCommand>
{
    public async Task<Result> Handle(CreateTrainerCommand request, CancellationToken cancellationToken)
    {
        // Integration events can be delivered more than once; an existing trainer is not an error.
        if (await trainerRepository.GetAsync(request.TrainerId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        trainerRepository.Insert(Trainer.Create(request.TrainerId, request.Name));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
