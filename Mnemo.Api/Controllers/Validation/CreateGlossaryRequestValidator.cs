using FluentValidation;
using Mnemo.Contracts.Glossary.Requests;
using Mnemo.Controllers.Validation.Rules;

namespace Mnemo.Controllers.Validation
{
    public class CreateGlossaryRequestValidator : AbstractValidator<CreateGlossaryRequest>
    {
        public CreateGlossaryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Glossary name is required")
                .Must(GlossaryRules.IsValidName)
                .WithMessage("Glossary name is required");
        }
    }
}
