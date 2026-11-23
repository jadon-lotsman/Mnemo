using FluentValidation;
using Mnemo.Contracts.Entry.Requests;
using Mnemo.Contracts.Glossary.Requests;

namespace Mnemo.Controllers.Validation
{
    public class PatchGlossaryRequestValidator : AbstractValidator<PatchGlossaryRequest>
    {
        public PatchGlossaryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Glossary name is required")
                .Must(GlossaryRules.IsValidName)
                .WithMessage("Glossary name is required");
        }
    }
}
