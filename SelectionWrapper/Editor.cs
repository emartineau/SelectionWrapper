using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace SelectionWrapper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EditorListener : IWpfTextViewCreationListener
    {
        private IWpfTextView _textView;

        private NormalizedSnapshotSpanCollection snapshotSpans;
        private ITextSelection textSelection;
        private int selectionLength;


        private Dictionary<char, char> charPairs = new Dictionary<char, char>()
        {
            {'\'', '\''},
            {'\"', '\"'},
            {'(', ')' },
            {'[', ']' },
            {'{', '}' },
            {'`', '`' }
        };

        public void TextViewCreated(IWpfTextView textView)
        {
            _textView = textView;
            textView.TextBuffer.Changing += TextBuffer_Changing;
            textView.TextBuffer.PostChanged += TextBuffer_PostChanged;
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            if (selectionLength > 0)
            {
                selectionLength = 0;
                var textBuffer = sender as ITextBuffer;
                var currentPosition = textSelection.End.Position;
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
                    textEdit.Insert(textSelection.Start.Position, wrappedSelectionText);

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

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            textSelection = _textView.Selection;
            snapshotSpans = textSelection.SelectedSpans;
            selectionLength = snapshotSpans.Max(span => span.Length);
        }
    }
}
