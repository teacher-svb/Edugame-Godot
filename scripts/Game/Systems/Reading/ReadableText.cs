using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TnT.EduGame.Reading
{
    /// <summary>How the reading panel looks: a letter on paper, a notice on a board, or a plain panel.</summary>
    public enum ReadableLook { Plain, Letter, Notice }

    /// <summary>
    /// A longer text to read in the reading panel (R2): a letter, a notice. <see cref="Body"/> may hold several
    /// paragraphs and BBCode; a line with only <c>---</c> starts a new page.
    /// </summary>
    [GlobalClass, Tool]
    public partial class ReadableText : Resource
    {
        public const string PageBreak = "---";

        [Export] public string Title { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Body { get; set; } = "";
        [Export] public ReadableLook Look { get; set; } = ReadableLook.Plain;

        /// <summary>The body split at <c>---</c> lines, each page trimmed; empty pages are dropped, at least one page.</summary>
        public IReadOnlyList<string> Pages => SplitPages(Body);

        public static IReadOnlyList<string> SplitPages(string body)
        {
            var pages = new List<string>();
            var current = new List<string>();
            foreach (var line in (body ?? "").Replace("\r\n", "\n").Split('\n'))
            {
                if (line.Trim() == PageBreak)
                {
                    pages.Add(string.Join("\n", current).Trim());
                    current.Clear();
                }
                else
                    current.Add(line);
            }
            pages.Add(string.Join("\n", current).Trim());
            var nonEmpty = pages.Where(p => p.Length > 0).ToList();
            return nonEmpty.Count > 0 ? nonEmpty : [""];
        }
    }
}
