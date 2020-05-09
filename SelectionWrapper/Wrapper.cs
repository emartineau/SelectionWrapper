using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SelectionWrapper
{
    internal class Wrapper
    {
        private IEditorOperations editorOperations;

        public IEditorOperations EditorOperations
        {
            get => editorOperations;
            set
            {
                editorOperations = value;
                TextSelection = editorOperations.TextView.Selection;
            }
        }
        private ITextSelection TextSelection { get; set; }
        private NormalizedSnapshotSpanCollection SelectedSpans { get; set; }
        private SnapshotPoint SelectionStartPositionBeforeInput { get; set; }
        private Dictionary<char, char> CharacterPairs { get; } = new Dictionary<char, char>()
        {
            {'\'', '\''},
            {'\"', '\"'},
            {'(', ')' },
            {'[', ']' },
            {'{', '}' },
            {'`', '`' },
            {'<', '>' }
        };


        public Wrapper(IEditorOperations editorOperations)
        {
            EditorOperations = editorOperations;
        }

        public void Wrap(ITextBuffer textBuffer)
        {
            if (textBuffer == null || SelectedSpans == null || SelectedSpans.Count == 0 
                // don't wrap if selection is multi-cursor
                || (SelectedSpans.Count > 1 && TextSelection.Mode == TextSelectionMode.Stream))
            {
                return;
            }
            if (!SelectedSpans.All(span => span.IsEmpty))
            {
                var endOfSelection = TextSelection.End.Position;

                // check if the caret ends up in a different position from the initial selection's starting point
                // if the caret doesn't move, we assume the user's last input did not add to the buffer (e.g. deletion)
                int caretPositionAfterInput = TextSelection.TextView.Caret.Position.BufferPosition;
                if (endOfSelection.Position == 0 || SelectionStartPositionBeforeInput == caretPositionAfterInput)
                {
                    return;
                }
                endOfSelection = endOfSelection.Subtract(1);
                if (endOfSelection.Position == textBuffer.CurrentSnapshot.Length)
                {
                    return;
                }

                char leftCharacter = endOfSelection.GetChar();

                // Only wrap when the user types/pastes a single wrapping character
                if (CharacterPairs.ContainsKey(leftCharacter)
                    && ((TextSelection.Mode == TextSelectionMode.Box && SelectedSpans.First().Start.Position == EditorOperations.TextView.Selection.SelectedSpans.First().Start.Position - 1)
                    || (caretPositionAfterInput - 1 == SelectionStartPositionBeforeInput)))
                {
                    char rightCharacter = CharacterPairs[leftCharacter];
                    string replacingText = SelectedSpans.Aggregate(
                        new StringBuilder(),
                        (text, span) =>
                        {
                            var spanLine = span.Snapshot.GetLineFromPosition(span.Start);
                            string lineEnding = SelectedSpans.Count > 1 ? spanLine.GetLineBreakText() : string.Empty;
                            StringBuilder replacingSpan = text.Append($"{span.GetText()}{rightCharacter}{lineEnding}");
                            return replacingSpan;
                        }, sb => sb.ToString());

                    if (TextSelection.Mode == TextSelectionMode.Box)
                    {
                        VirtualSnapshotPoint boxSelectionStart, boxSelectionEnd;
                        EditorOperations.InsertTextAsBox(replacingText, out boxSelectionStart, out boxSelectionEnd);
                    }
                    else
                    {
                        EditorOperations.InsertText(replacingText);
                    }
                }
            }
        }
        public void CaptureSelectionState()
        {
            SelectedSpans = EditorOperations.TextView?.Selection?.SelectedSpans;
            SelectionStartPositionBeforeInput = TextSelection.Start.Position;
        }
    }
}
