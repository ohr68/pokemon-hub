using FluentValidation;
using PokeHub.Modules.Games.Domain.SaveFiles;

namespace PokeHub.Modules.Games.Application.SaveFiles.UploadSaveFile;

internal sealed class UploadSaveFileCommandValidator : AbstractValidator<UploadSaveFileCommand>
{
    public UploadSaveFileCommandValidator()
    {
        RuleFor(c => c.Content)
            .NotEmpty()
            .Must(content => content.Length <= SaveFile.MaxSizeBytes)
            .WithMessage($"A save file can be at most {SaveFile.MaxSizeBytes / 1024} KB.");

        RuleFor(c => c.Label).MaximumLength(100);
    }
}
