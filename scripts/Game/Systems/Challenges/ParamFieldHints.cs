using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Question
{
    // Resources with string fields that name a formula param (visualizations).
    // The owning MathChallenge passes its param names down, so the inspector can
    // offer them as a dropdown instead of free text.
    public interface IParamFieldOwner
    {
        void SetAvailableParams(string[] names);
    }

    public static class ParamFieldHints
    {
        // turns the listed string fields into a dropdown of `available` (typing a
        // custom value stays possible); no-op until the names are known
        public static void Apply(Godot.Collections.Dictionary property, IEnumerable<string> fields, string[] available)
        {
            if (available is not { Length: > 0 } || !fields.Contains(property["name"].AsString()))
                return;

            property["hint"] = (int)PropertyHint.EnumSuggestion;
            property["hint_string"] = string.Join(",", available);
        }
    }
}
