using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionWrapper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EditorListener : IWpfTextViewCreationListener
    {
        Selection selection;

        [Import]
        ITextDifferencingSelectorService TextDifferencingService = null;

        IWpfTextView _textView;

        ITextSnapshot _snapshot;
        int changeIndex;
        string span;
        ITextSelection textSelection;

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
            selection = new Selection(textView.Selection);
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            var textBuffer = sender as ITextBuffer;
            var textEdit = textBuffer.CreateEdit();
            char leftChar = textSelection.End.Position.Subtract(1).GetChar();
            if (charPairs.ContainsKey(leftChar))
            {
                char rightChar = charPairs[leftChar];
                string wrappedSelectionText = (span.Length == 0) ? "" : $"{span}{rightChar}";
                textEdit.Insert(textSelection.Start.Position, wrappedSelectionText);
            }

            if (textEdit.HasEffectiveChanges)
            {
                textEdit.Apply();
            }

            textEdit.Dispose();
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            //if (e.EditTag is ITypingEditTag)
            //{

            //var currentBuffer = sender as ITextBuffer;
            //var previousBuffer = e.Before.TextBuffer;
            //var defaultDifferencingService = TextDifferencingService.DefaultTextDifferencingService;
            
            //var difference = defaultDifferencingService.DiffStrings(
            //  currentBuffer.CurrentSnapshot.GetText(),
            //previousBuffer.CurrentSnapshot.GetText(),
            //new StringDifferenceOptions());
            
            //changeIndex = _textView.Selection.Start.Position;
            //span = _textView.Selection.StreamSelectionSpan.SnapshotSpan;

            textSelection = _textView.Selection;
            span = textSelection.SelectedSpans.First().GetText();

            //currentBuffer.CurrentSnapshot.CreateTrackingPoint(changeIndex.Position.Snapshot)
            //selection.Wrap();
            //}
        }
    }
}
