using ICSharpCode.TextEditor.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.AddOns
{
    public class JSONHighlightingStrategy : IHighlightingStrategy
    {
        public string[] Extensions
        {
            get { throw new NotImplementedException(); }
        }

        public HighlightColor GetColorFor(string name)
        {
            throw new NotImplementedException();
        }

        public void MarkTokens(IDocument document)
        {
            throw new NotImplementedException();
        }

        public void MarkTokens(IDocument document, List<LineSegment> lines)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<string, string> Properties
        {
            get { throw new NotImplementedException(); }
        }
    }
}
