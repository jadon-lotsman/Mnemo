using Mnemo.Contracts.Glossary.Requests;
using Mnemo.Entities;
using Mnemo.Shared.Enums;

namespace Mnemo.Shared.Extensions
{
    public static class GlossaryExstension
    {
        public static bool TryPatch(this Glossary gloss, PatchGlossaryRequest patch)
        {
            string? _name = null;
            if (patch.Name != null)
            {
                var normalized = gloss.Name.RemoveMultispaces().Capitalize();
                if (string.IsNullOrWhiteSpace(normalized))
                    return false;

                _name = normalized;
            }


            if (_name != null)
                gloss.Name = _name;

            return true;
        }
    }
}
