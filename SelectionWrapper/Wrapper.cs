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
        private int SelectionStartPositionBeforeInput { get; set; }
        private string SelectedText { get; set; }
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
            if (textBuffer == null)
            {
                return;
            }
            if (SelectedText.Length > 0)
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

                if (CharacterPairs.ContainsKey(leftCharacter))
                {
                    char rightCharacter = CharacterPairs[leftCharacter];
                    string wrappedSelectionText = $"{SelectedText}{rightCharacter}";

                    EditorOperations.InsertText(wrappedSelectionText);
                }
            }
        }
        public void CaptureSelectionState()
        {
            SelectedText = EditorOperations.SelectedText;
            SelectionStartPositionBeforeInput = TextSelection.Start.Position;
        }
    }
}
