using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// Szablon elementu Kontrolka użytkownika jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=234236

namespace WordPad.WordPadUI
{
    public sealed partial class TextRuler : UserControl
    {
        public RichEditBox editor;
        public ScrollViewer editorScroll;

        public Slider ZoomSlider { get; private set; }

        public TextRuler()
        {
            this.InitializeComponent();
        }
        public string GetText(RichEditBox RichEditor)
        {
            RichEditor.Document.GetText(TextGetOptions.FormatRtf, out string Text);
            ITextRange Range = RichEditor.Document.GetRange(0, Text.Length);
            Range.GetText(TextGetOptions.FormatRtf, out string Value);
            return Value;
        }

        private void SCR3_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ZoomSlider.Value = editorScroll.ZoomFactor;
        }

        private void TabIndent_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SetParagraphIndents((float)LeftInd.Value, (float)RightInd.Value, (float)TabIndent.Value, true);
        }

        private void SetParagraphIndents(float leftIndent, float rightIndent, float firstLineIndent, bool applyToSelectionOnly = true)
        {
            // Get the ITextDocument interface for the RichEditBox's document
            var document = editor.Document;

            // Get the current selection
            var selection = document.Selection;
            
            // Get the current selection's start and end positions
            int start = selection.StartPosition;
            int end = selection.EndPosition;

            // If applyToSelectionOnly is true, check if there's any selected text in the RichEditBox
            if (applyToSelectionOnly && start == end)
            {
                //return;
            }

            // Get the ITextRange interface for the selection or the entire document
            ITextRange textRange;
            if (applyToSelectionOnly)
            {
                textRange = selection;
            }
            else
            {
                textRange = document.GetRange(0, GetText(editor).Length);
            }

            // Get the ITextParagraphFormat interface for the text range
            var paragraphFormat = textRange.ParagraphFormat;

            // Set the left and right indents for the current selection's paragraph(s)
            try
            {
                if (selection.Length != 0)
                {
                    paragraphFormat.SetIndents(firstLineIndent, leftIndent, rightIndent);
                }
                else
                {
                    document.GetRange(selection.StartPosition, selection.EndPosition + 1);
                    paragraphFormat.SetIndents(firstLineIndent, leftIndent, rightIndent);
                }
            }
            catch
            {

            }

            // Apply the new paragraph format to the current selection or the entire document
            textRange.ParagraphFormat = paragraphFormat;

            // LeftIndent.Text = leftIndent.ToString();

            // RightIndent.Text = rightIndent.ToString();
        }
        private void LeftInd_ValueChanged(object Sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs EvArgs)
        {
            SetParagraphIndents((float)LeftInd.Value, (float)RightInd.Value, (float)TabIndent.Value, true);
        }

        private void RightInd_ValueChanged(object Sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs EvArgs)
        {
            SetParagraphIndents((float)LeftInd.Value, (float)RightInd.Value, (float)TabIndent.Value, true);
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            shadow.Receivers.Add(test);
        }
    }
}
