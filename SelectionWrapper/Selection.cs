using Microsoft.VisualStudio.Text.Differencing;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionWrapper
{
    internal class Selection
    {
        //[Import]
        ITextSelection TextSelection { get; set; }


        public Selection(ITextSelection textSelection)
        {
            TextSelection = textSelection;
        }

        public void Wrap()
        {
            if (!TextSelection.IsActive || TextSelection.IsEmpty)
            {
                return;
            }

            var leftInsertionPoint = TextSelection.Start.Position;
            var rightInsertionPoint = TextSelection.End.Position;
            var textBuffer = TextSelection.TextView.TextBuffer;
            char left = leftInsertionPoint.GetChar();

            textBuffer.TakeThreadOwnership();
            var textEdit = textBuffer.CreateEdit();
            textEdit.Insert(leftInsertionPoint, left.ToString());
            textEdit.Insert(rightInsertionPoint, left.ToString());

            if (textEdit.HasEffectiveChanges)
            {
                textEdit.Apply();
            }

            textEdit.Dispose();
        }
    }
}
