using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowBlockViewItem
    {
        private int _defaultWidth = 150;
        private int _defaultHeight = 120;

        public FlowBlockViewItem()
        {
            Width = _defaultWidth;
            Height = _defaultHeight;
            Blocks = new List<FlowBlockViewItem>();
        }

        public string Id { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public string Scope { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public bool IsQuestion
        {
            get { return Blocks.Count == 0; }
        }


        public List<FlowBlockViewItem> Blocks { get; set; }

        public string Style
        {
            get
            {
                return string.Format("top: {0}px; left: {1}px; height: {2}px; width:{3}px", Top, Left, Height, Width);
            }
        }

        public int InnerHeight
        {
            get
            {
                if (IsQuestion)
                    return Height;
                return Blocks.Select(block => block.Top + block.InnerHeight).Max();
            }
        }
        public int InnerWidth
        {
            get
            {
                if (IsQuestion)
                    return Width;
                return Blocks.Select(block => block.Left + block.InnerWidth).Max();
            }
        }
    }
}
