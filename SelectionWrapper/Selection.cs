using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SelectionWrapper
{
    internal class Selection
    {
        ITextSelection TextSelection { get; set; }
        private NormalizedSnapshotSpanCollection snapshotSpans;
        private int selectionLength;
        private Dictionary<char, char> charPairs = new Dictionary<char, char>()
        {
            {'\'', '\''},
            {'\"', '\"'},
            {'(', ')' },
            {'[', ']' },
            {'{', '}' },
            {'`', '`' },
            {'<', '>' }
        };


        public Selection(ITextSelection textSelection)
        {
            TextSelection = textSelection;
        }

        public void Wrap(ITextBuffer textBuffer)
        {
            if (textBuffer == null)
            {
                return;
            }
            if (selectionLength > 0)
            {
                selectionLength = 0;
                var currentPosition = TextSelection.End.Position;
                if (currentPosition.Position == 0)
                {
                    return;
                }
                currentPosition = currentPosition.Subtract(1);
                if (currentPosition.Position == textBuffer.CurrentSnapshot.Length)
                {
                    return;
                }

                char leftChar = currentPosition.GetChar();

                if (charPairs.ContainsKey(leftChar))
                {
                    char rightChar = charPairs[leftChar];
                    var selectedText = new StringBuilder();
                    selectedText = snapshotSpans.Aggregate(
                        selectedText,
                        (spansAsTextSoFar, span) => spansAsTextSoFar.Append(span.GetText()));
                    string wrappedSelectionText = $"{selectedText.ToString()}{rightChar}";

                    var textEdit = textBuffer.CreateEdit();
                    textEdit.Insert(TextSelection.Start.Position, wrappedSelectionText);

                    if (textEdit.HasEffectiveChanges)
                    {
                        textEdit.Apply();
                    }
                    else
                    {
                        textEdit.Cancel();
                    }

                    textEdit.Dispose();
                }
            }
        }
        public void CaptureSelectionState()
        {
            snapshotSpans = TextSelection.SelectedSpans;
            selectionLength = snapshotSpans.Max(span => span.Length);
        }
    }
}
