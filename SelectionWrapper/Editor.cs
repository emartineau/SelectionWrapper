using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace SelectionWrapper
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EditorListener : IWpfTextViewCreationListener
    {
        private IWpfTextView _textView;

        public Wrapper Wrapper { get; private set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            _textView = textView;
            _textView.GotAggregateFocus += OnTextViewFocus;
            _textView.TextBuffer.Changing += TextBuffer_Changing;
            _textView.TextBuffer.PostChanged += TextBuffer_PostChanged;

            Wrapper = new Wrapper(_textView.Selection);
        }

        void OnTextViewFocus(object sender, EventArgs e)
        {
            var focusedTextView = sender as IWpfTextView;
            if (focusedTextView != null)
            {
                _textView = focusedTextView;
            }
        }

        private void TextBuffer_PostChanged(object sender, EventArgs e)
        {
            var textBuffer = sender as ITextBuffer;
            Wrapper.Wrap(textBuffer, _textView.Caret.Position.BufferPosition);
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            Wrapper.TextSelection = _textView.Selection;
            Wrapper.CaptureSelectionState();
        }
    }
}
