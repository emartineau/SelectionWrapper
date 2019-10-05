using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SelectionWrapper
{
    internal class Wrapper
    {
        public ITextSelection TextSelection { get; set; }
        private NormalizedSnapshotSpanCollection snapshotSpans;
        private int selectionStartPositionBeforeInput;
        private int selectionLength;
        private Dictionary<char, char> characterPairs = new Dictionary<char, char>()
        {
            {'\'', '\''},
            {'\"', '\"'},
            {'(', ')' },
            {'[', ']' },
            {'{', '}' },
            {'`', '`' },
            {'<', '>' }
        };


        public Wrapper(ITextSelection textSelection)
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
                var endOfSelection = TextSelection.End.Position;

                // check if the caret ends up in a different position from the initial selection's starting point
                // if the caret doesn't move, we assume the user's last input did not add to the buffer (e.g. deletion)
                int caretPositionAfterInput = TextSelection.TextView.Caret.Position.BufferPosition;
                if (endOfSelection.Position == 0 || selectionStartPositionBeforeInput == caretPositionAfterInput)
                {
                    return;
                }
                endOfSelection = endOfSelection.Subtract(1);
                if (endOfSelection.Position == textBuffer.CurrentSnapshot.Length)
                {
                    return;
                }

                char leftCharacter = endOfSelection.GetChar();

                if (characterPairs.ContainsKey(leftCharacter))
                {
                    char rightCharacter = characterPairs[leftCharacter];
                    var selectedText = new StringBuilder();

                    foreach (var span in snapshotSpans)
                    {
                        selectedText.Append(span.GetText());
                    }

                    string wrappedSelectionText = $"{selectedText.ToString()}{rightCharacter}";

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
            selectionStartPositionBeforeInput = TextSelection.Start.Position;
            snapshotSpans = TextSelection.SelectedSpans;
            selectionLength = snapshotSpans.Max(span => span.Length);
        }
    }
}
