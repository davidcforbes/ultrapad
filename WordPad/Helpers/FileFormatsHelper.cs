using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPad.Helpers
{
    internal class FileFormatsHelper
    {
        #region DOCX

        private readonly Document _document;

        public string ConvertToRtf()
        {
            // First pass: collect all unique colors used in the document
            var uniqueColors = new HashSet<string>();
            foreach (var run in _document.Descendants<Run>())
            {
                if (run.RunProperties?.Color != null)
                {
                    uniqueColors.Add(run.RunProperties.Color.Val);
                }
            }

            // Build color index map
            var colorIndexMap = new Dictionary<string, int>();
            int colorIndex = 1; // RTF color table starts at index 1 (0 is auto)
            foreach (var colorHex in uniqueColors)
            {
                colorIndexMap[colorHex] = colorIndex++;
            }

            var rtfWriter = new StringWriter();
            rtfWriter.WriteLine("{\\rtf1\\ansi\\deff0");

            // Define color table with ONLY colors actually used in the document
            rtfWriter.WriteLine("{\\colortbl ;"); // First entry is auto/default

            foreach (var colorHex in uniqueColors)
            {
                if (!string.IsNullOrEmpty(colorHex) && colorHex.Length == 6)
                {
                    try
                    {
                        int red = Convert.ToInt32(colorHex.Substring(0, 2), 16);
                        int green = Convert.ToInt32(colorHex.Substring(2, 2), 16);
                        int blue = Convert.ToInt32(colorHex.Substring(4, 2), 16);
                        rtfWriter.WriteLine($"\\red{red}\\green{green}\\blue{blue};");
                    }
                    catch
                    {
                        // Invalid color format, skip
                    }
                }
            }

            rtfWriter.WriteLine("}");

            // Second pass: write the document content
            foreach (var paragraph in _document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                rtfWriter.WriteLine("{\\pard");

                foreach (var run in paragraph.Elements<Run>())
                {
                    if (run.RunProperties != null)
                    {
                        if (run.RunProperties.Bold != null && run.RunProperties.Bold.Val)
                        {
                            rtfWriter.Write("\\b ");
                        }

                        if (run.RunProperties.Italic != null && run.RunProperties.Italic.Val)
                        {
                            rtfWriter.Write("\\i ");
                        }

                        if (run.RunProperties.Color != null)
                        {
                            var colorHex = run.RunProperties.Color.Val;
                            if (colorIndexMap.TryGetValue(colorHex, out int index))
                            {
                                rtfWriter.Write($"\\cf{index} ");
                            }
                        }

                        if (run.RunProperties.FontSize != null)
                        {
                            var fontSize = run.RunProperties.FontSize.Val;
                            rtfWriter.Write($"\\fs{fontSize} ");
                        }
                    }

                    foreach (var text in run.Elements<Text>())
                    {
                        rtfWriter.Write(text.Text);
                    }

                    if (run.RunProperties != null && ((run.RunProperties.Bold != null && run.RunProperties.Bold.Val) ||
                        (run.RunProperties.Italic != null && run.RunProperties.Italic.Val)))
                    {
                        rtfWriter.Write("\\b0\\i0 ");
                    }
                }

                rtfWriter.WriteLine("}");
            }

            rtfWriter.WriteLine("}");

            return rtfWriter.ToString();
        }
        #endregion



        #region RTF
        #endregion

    }
}

