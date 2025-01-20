using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace PdfEmbedding.Services
{
    public class DocxProcessingService
    {
        // Extract text from a DOCX file
        public string ExtractTextFromDocx(string filePath)
        {
            StringBuilder text = new StringBuilder();

            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Loop through paragraphs
                    foreach (var paragraph in body.Elements<Paragraph>())
                    {
                        // Loop through each run in the paragraph
                        foreach (var run in paragraph.Elements<Run>())
                        {
                            // Correct way to access text in a run
                            foreach (var textElement in run.Elements<Text>())
                            {
                                text.Append(textElement.Text);
                            }
                        }
                        text.AppendLine(); // Add a newline after each paragraph
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception or log it
                throw new InvalidOperationException($"Error reading DOCX file: {ex.Message}", ex);
            }

            return text.ToString();
        }

        // Split the extracted text into smaller chunks (500 characters per chunk)
        public List<string> ChunkText(string text, int chunkSize = 500)
        {
            List<string> chunks = new List<string>();
            int totalLength = text.Length;

            for (int i = 0; i < totalLength; i += chunkSize)
            {
                var chunk = text.Substring(i, Math.Min(chunkSize, totalLength - i));
                chunks.Add(chunk);
            }

            return chunks;
        }
    }
}
