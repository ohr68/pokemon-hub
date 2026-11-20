using FluentValidation;

namespace PokeHub.Modules.Games.Application.Games.RegisterGame;

internal sealed class RegisterGameCommandValidator : AbstractValidator<RegisterGameCommand>
{
    public RegisterGameCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);

        RuleFor(c => c.Slug)
            .NotEmpty()
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Use lowercase letters, numbers and dashes, like my-first-game.")
            .MaximumLength(40);

        RuleFor(c => c.Description).MaximumLength(1000);

        RuleFor(c => c.Url).NotNull().Must(url => url.IsAbsoluteUri);

        RuleFor(c => c.RedirectUris).NotEmpty();

        RuleForEach(c => c.RedirectUris).Must(BeAbsoluteHttpUrl).WithMessage("Each redirect URI must be an http(s) URL.");

        RuleFor(c => c.WebOrigins).NotEmpty();

        RuleForEach(c => c.WebOrigins).Must(BeOrigin).WithMessage("Each origin must be scheme://host[:port] without a path.");
    }

    private static bool BeAbsoluteHttpUrl(string value) =>
        Uri.TryCreate(value.Replace("*", "x", StringComparison.Ordinal), UriKind.Absolute, out Uri? uri)
        && uri.Scheme is "http" or "https";

    private static bool BeOrigin(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
        && uri.Scheme is "http" or "https"
        && uri.AbsolutePath == "/"
        && !value.EndsWith('/');
}
