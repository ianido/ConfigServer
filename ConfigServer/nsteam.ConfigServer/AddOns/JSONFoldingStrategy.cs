using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.Addons
{

    public class JSONFoldingStrategy : IFoldingStrategy
    {
        public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInformation)
        {
            // This is a simple folding strategy.
            // It searches for matching brackets ('{', '}') and creates folds
            // for each region.

            List<FoldMarker> foldMarkers = new List<FoldMarker>();
            for (int offset = 0; offset < document.TextLength; ++offset)
            {
                char c = document.GetCharAt(offset);
                if (c == '{')
                {
                    int offsetOfClosingBracket = document.FormattingStrategy.SearchBracketForward(document, offset + 1, '{', '}');
                    if (offsetOfClosingBracket > 0)
                    {
                        int length = offsetOfClosingBracket - offset + 1;
                        foldMarkers.Add(new FoldMarker(document, offset, length, "{...}", false));
                    }
                }
            }
            return foldMarkers;
        }

    }
}