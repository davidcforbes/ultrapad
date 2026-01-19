using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

namespace WordPad.Helpers
{
    public class OdtHelper
    {
        // Helper method to safely load XML with XXE protection
        private XmlDocument LoadXmlSafely(string xmlContent)
        {
            var xmlDocument = new XmlDocument();
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersFromEntities = 1024,
                MaxCharactersInDocument = 10485760 // 10MB limit
            };

            using (var stringReader = new StringReader(xmlContent))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                xmlDocument.Load(xmlReader);
            }

            return xmlDocument;
        }

        // Sanitize input for XPath queries to prevent injection
        private string SanitizeXPathInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Only allow alphanumeric, hyphens, underscores, and dots
            return Regex.Replace(input, @"[^a-zA-Z0-9\-_\.]", "");
        }

        // Validate and sanitize image paths to prevent path traversal
        private bool IsValidImagePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Prevent path traversal attacks
            if (path.Contains("..") || path.Contains("\\") || Path.IsPathRooted(path))
                return false;

            // Only allow images in Pictures or similar safe directories
            var normalizedPath = path.Replace("/", "\\").ToLowerInvariant();
            if (normalizedPath.Contains("system") || normalizedPath.Contains("windows"))
                return false;

            return true;
        }
        private void ApplyTextStyle(ITextSelection selection, string text, string styleName, string stylesXml, RichEditBox Editor)
        {
            // Insert the text
            selection.TypeText(text);

            // Get the start and end positions of the inserted text
            var startPosition = selection.StartPosition - text.Length;
            var endPosition = selection.StartPosition;

            // Get the ITextRange for the inserted text
            var document = Editor.Document;
            ITextRange range = document.GetRange(startPosition, endPosition);

            if (string.IsNullOrEmpty(styleName)) return;

            // Sanitize styleName to prevent XPath injection
            var sanitizedStyleName = SanitizeXPathInput(styleName);
            if (string.IsNullOrEmpty(sanitizedStyleName)) return;

            // Parse styles.xml to extract style details using secure XML loading
            var stylesDoc = LoadXmlSafely(stylesXml);

            var namespaceManager = new XmlNamespaceManager(stylesDoc.NameTable);
            namespaceManager.AddNamespace("style", "urn:oasis:names:tc:opendocument:xmlns:style:1.0");
            namespaceManager.AddNamespace("fo", "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0");

            var styleNode = stylesDoc.SelectSingleNode($"//style:style[@style:name='{sanitizedStyleName}']", namespaceManager);
            if (styleNode != null)
            {
                // Extract style attributes: Bold, Italic, Underline
                var fontWeight = styleNode.SelectSingleNode(".//fo:font-weight", namespaceManager)?.InnerText;
                var fontStyle = styleNode.SelectSingleNode(".//fo:font-style", namespaceManager)?.InnerText;
                var textDecoration = styleNode.SelectSingleNode(".//style:text-underline-style", namespaceManager)?.InnerText;

                if (fontWeight == "bold") range.CharacterFormat.Bold = FormatEffect.On;
                if (fontStyle == "italic") range.CharacterFormat.Italic = FormatEffect.On;
                if (textDecoration == "solid") range.CharacterFormat.Underline = UnderlineType.Single;
            }
        }

        public IRandomAccessStream ConvertToRandomAccessStream(Stream inputStream)
        {
            // Convert Stream (System.IO.Stream) to IRandomAccessStream
            return inputStream.AsRandomAccessStream();
        }

        // Helper method to extract plain text from ODT content.xml
        public string ExtractPlainTextFromODT(string xmlContent)
        {
            // Simple parsing to extract text between <text:p> tags
            var plainTextBuilder = new StringBuilder();
            var xmlDocument = LoadXmlSafely(xmlContent);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");

            var paragraphs = xmlDocument.SelectNodes("//text:p", namespaceManager);
            foreach (XmlNode paragraph in paragraphs)
            {
                plainTextBuilder.AppendLine(paragraph.InnerText);
            }

            return plainTextBuilder.ToString();
        }


        // Load ODT content and apply styles
        public async Task LoadOdtContentWithStyling(string contentXml, string stylesXml, System.IO.Compression.ZipArchive archive, RichEditBox Editor)
        {
            var contentDoc = LoadXmlSafely(contentXml);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(contentDoc.NameTable);
            namespaceManager.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            namespaceManager.AddNamespace("draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
            namespaceManager.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");

            // Get all paragraphs
            var paragraphs = contentDoc.SelectNodes("//text:p", namespaceManager);

            // Clear the RichEditBox
            Editor.Document.SetText(TextSetOptions.None, "");

            var document = Editor.Document;
            var selection = document.Selection;

            foreach (XmlNode paragraph in paragraphs)
            {
                foreach (XmlNode childNode in paragraph.ChildNodes)
                {
                    if (childNode.Name == "text:span")
                    {
                        // Handle styled text
                        var textContent = childNode.InnerText;
                        var styleName = childNode.Attributes["text:style-name"]?.Value;
                        ApplyTextStyle(selection, textContent, styleName, stylesXml, Editor);
                    }
                    else if (childNode.Name == "draw:frame")
                    {
                        // Handle images
                        var imageNode = childNode.SelectSingleNode(".//draw:image", namespaceManager);
                        if (imageNode != null)
                        {
                            var imagePath = imageNode.Attributes["xlink:href"]?.Value?.TrimStart('/');

                            // Validate image path to prevent path traversal
                            if (!string.IsNullOrEmpty(imagePath) && IsValidImagePath(imagePath))
                            {
                                var imageEntry = archive.GetEntry(imagePath);
                                if (imageEntry != null)
                                {
                                    // Validate image size (max 10MB)
                                    if (imageEntry.Length > 10 * 1024 * 1024)
                                        continue;

                                    using (var imageStream = imageEntry.Open())
                                    {
                                        if (imageStream != null)
                                        {
                                            IRandomAccessStream imageUWPFormattedStream = ConvertToRandomAccessStream(imageStream);

                                            // Use reasonable default dimensions
                                            int width = Math.Min(512, 512);
                                            int height = Math.Min(512, 512);

                                            // Load the file into the Document property of the RichEditBox.
                                            Editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "img", imageUWPFormattedStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Add a paragraph break
                selection.TypeText(Environment.NewLine);
            }
        }
        public async Task LoadOdtContentIntoRichEditBox(string contentXml, string stylesXml, System.IO.Compression.ZipArchive archive, RichEditBox Editor)
        {
            var xmlDocument = LoadXmlSafely(contentXml);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            namespaceManager.AddNamespace("text", "urn:oasis:names:tc:opendocument:xmlns:text:1.0");
            namespaceManager.AddNamespace("draw", "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
            namespaceManager.AddNamespace("office", "urn:oasis:names:tc:opendocument:xmlns:office:1.0");

            // Access paragraphs
            var paragraphs = xmlDocument.SelectNodes("//text:p", namespaceManager);

            // Start loading into RichEditBox
            Editor.Document.SetText(TextSetOptions.FormatRtf, "");

            var document = Editor.Document;
            var selection = document.Selection;

            foreach (XmlNode paragraph in paragraphs)
            {
                // Extract and style text
                foreach (XmlNode childNode in paragraph.ChildNodes)
                {
                    if (childNode.Name == "text:span")
                    {
                        // Handle styled text
                        var textContent = childNode.InnerText;
                        var style = childNode.Attributes["text:style-name"]?.Value;

                        // Apply styles based on style definitions (styles.xml)
                        ApplyTextStyle(selection, textContent, style, stylesXml, Editor);
                    }
                    else if (childNode.Name == "draw:frame")
                    {
                        // Handle embedded images
                        var imageNode = childNode.SelectSingleNode(".//draw:image", namespaceManager);
                        if (imageNode != null)
                        {
                            var imagePath = imageNode.Attributes["xlink:href"]?.Value?.TrimStart('/');

                            // Validate image path to prevent path traversal
                            if (!string.IsNullOrEmpty(imagePath) && IsValidImagePath(imagePath))
                            {
                                var imageEntry = archive.GetEntry(imagePath);
                                if (imageEntry != null)
                                {
                                    // Validate image size (max 10MB)
                                    if (imageEntry.Length > 10 * 1024 * 1024)
                                        continue;

                                    using (var imageStream = imageEntry.Open())
                                    {
                                        if (imageStream != null)
                                        {
                                            IRandomAccessStream imageUWPFormattedStream = ConvertToRandomAccessStream(imageStream);

                                            // Use reasonable default dimensions
                                            int width = Math.Min(512, 512);
                                            int height = Math.Min(512, 512);

                                            // Load the file into the Document property of the RichEditBox.
                                            Editor.Document.Selection.InsertImage(width, height, 0, VerticalCharacterAlignment.Baseline, "img", imageUWPFormattedStream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Add a paragraph break
                selection.TypeText(Environment.NewLine);
            }
        }
    }
}

