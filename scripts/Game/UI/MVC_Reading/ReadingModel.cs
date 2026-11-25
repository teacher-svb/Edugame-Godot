using System.Collections.Generic;
using Godot;
using TnT.EduGame.Reading;

namespace TnT.EduGame.UI
{
    /// <summary>What the reading panel shows: the text, split into pages, and the page the player is on.</summary>
    [GlobalClass]
    public partial class ReadingModel : Node
    {
        public ReadableText Text { get; private set; }
        public IReadOnlyList<string> Pages { get; private set; } = [""];
        public int PageIndex { get; private set; }

        public string Title => Text?.Title ?? "";
        public ReadableLook Look => Text?.Look ?? ReadableLook.Plain;
        public int PageCount => Pages.Count;
        public string CurrentPage => Pages[PageIndex];
        public bool IsLastPage => PageIndex >= Pages.Count - 1;

        /// <summary>True once the last page has been shown: the text has been read to the end.</summary>
        public bool ReachedEnd { get; private set; }

        public void SetText(ReadableText text)
        {
            Text = text;
            Pages = text?.Pages ?? [""];
            PageIndex = 0;
            ReachedEnd = IsLastPage;
        }

        /// <summary>Goes to the next page; false when already on the last one.</summary>
        public bool NextPage()
        {
            if (IsLastPage)
                return false;
            PageIndex++;
            ReachedEnd |= IsLastPage;
            return true;
        }
    }
}
