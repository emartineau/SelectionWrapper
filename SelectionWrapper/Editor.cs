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

        public Selection Selection { get; private set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            _textView = textView;
            _textView.TextBuffer.Changing += TextBuffer_Changing;
            _textView.TextBuffer.PostChanged += TextBuffer_PostChanged;
            Selection = new Selection(textView.Selection);
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            var textBuffer = sender as ITextBuffer;
            Selection.Wrap(textBuffer);
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            Selection.CaptureSelectionState();
        }
    }
}
