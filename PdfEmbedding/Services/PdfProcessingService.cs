using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace PdfEmbedding.Services
{
    public class PdfProcessingService
    {
        public string ExtractTextFromPdf(string filePath)
        {
            StringBuilder text = new StringBuilder();

            using (var reader = new PdfReader(filePath))
            {
                using (var pdfDoc = new PdfDocument(reader))
                {
                    var numberOfPages = pdfDoc.GetNumberOfPages();
                    for (int pageNumber = 1; pageNumber <= numberOfPages; pageNumber++)
                    {
                        var page = pdfDoc.GetPage(pageNumber);
                        var strategy = new LocationTextExtractionStrategy();
                        var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
                        text.AppendLine(currentText);
                    }
                }
            }

            return text.ToString();
        }

        // Split the extracted text into smaller chunks (for example, 500 characters per chunk)
        public List<string> ChunkText(string text)
        {
            List<string> chunks = new List<string>();
            int chunkSize = 500; // Adjust this value based on your needs
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
