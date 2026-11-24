using Mnemo.Shared.Enums;

namespace Mnemo.Controllers.Validation.Rules
{
    public static class GlossaryRules
    {
        public static bool IsValidName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var trimmed = name.Trim();
            if (!trimmed.Any(char.IsLetter))
                return false;

            if (trimmed.Length < 3 || trimmed.Length > 45)
                return false;

            return true;
        }
    }
}
