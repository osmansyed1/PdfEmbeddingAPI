using Microsoft.AspNetCore.Mvc;
using PdfEmbedding.Models;
using PdfEmbedding.Services;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

[Route("api/[controller]")]
[ApiController]
public class PdfEmbeddingsController : ControllerBase
{
    private readonly PdfProcessingService _pdfProcessingService;
    private readonly EmbeddingService _embeddingService;
    private readonly StorageService _storageService;
    private readonly OpenAIService _openAIService;

    public PdfEmbeddingsController(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not found in configuration");
        }

        _pdfProcessingService = new PdfProcessingService();
        _embeddingService = new EmbeddingService(apiKey);
        _storageService = new StorageService("Vectors");
        _openAIService = new OpenAIService(apiKey);
    }

    // Upload PDF, extract text, generate embeddings, and save them
    [HttpPost("upload-pdf")]
    public async Task<IActionResult> UploadPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Extract text from the uploaded PDF
        var text = _pdfProcessingService.ExtractTextFromPdf(filePath);

        // Dynamically chunk the text based on the size of the content (500 characters per chunk)
        var chunks = _pdfProcessingService.ChunkText(text, 500); // 500 characters per chunk

        // Generate embeddings for each chunk
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks);

        // Save the embeddings and chunks to storage
        _storageService.SaveVectors(file.FileName + ".json", chunks, embeddings);

        return Ok("File uploaded and processed successfully.");
    }

    // Ask a question, find matching embeddings, and generate an answer
    [HttpPost("ask-question")]
    public async Task<IActionResult> AskQuestion([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrEmpty(request.Question))
            return BadRequest("Question cannot be empty");

        try
        {
            // Load embeddings and chunks stored on disk
            var (chunks, embeddings) = _storageService.LoadVectors(request.FileName);

            if (!embeddings.Any())
                return BadRequest("No embeddings available to search");

            // Generate embedding for the question
            var queryEmbedding = await _embeddingService.GenerateQueryEmbeddingAsync(request.Question);

            // Find the top matching embeddings using cosine similarity
            int topK = 3; // Top-K number of matches to retrieve
            var topMatches = FindTopMatches(queryEmbedding, embeddings, topK);

            // Create the context from the top matching chunks
            var context = string.Join("\n", topMatches.Select(match => $"Chunk {match.Index + 1}: {chunks[match.Index]}"));

            // Generate an answer using OpenAI based on the context
            var generatedAnswer = await _openAIService.GenerateAnswer(request.Question, context);

            // Return the generated answer along with the similarity score (optional)
            return Ok(new
            {
                answer = generatedAnswer,
                confidence = Math.Round(topMatches.First().Similarity, 4),
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing question: {ex.Message}");
        }
    }

    // Find the top matching embeddings using cosine similarity
    private List<(int Index, float Similarity)> FindTopMatches(List<float> queryEmbedding, List<List<float>> embeddings, int topK)
    {
        var similarities = new List<(int Index, float Similarity)>();

        for (int i = 0; i < embeddings.Count; i++)
        {
            float similarity = CalculateCosineSimilarity(queryEmbedding, embeddings[i]);
            similarities.Add((i, similarity));
        }

        return similarities
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .ToList();
    }

    // Calculate cosine similarity between two vectors
    private float CalculateCosineSimilarity(List<float> vector1, List<float> vector2)
    {
        float dotProduct = 0;
        float magnitude1 = 0;
        float magnitude2 = 0;

        for (int i = 0; i < vector1.Count; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = (float)Math.Sqrt(magnitude1);
        magnitude2 = (float)Math.Sqrt(magnitude2);

        return magnitude1 * magnitude2 == 0 ? 0 : dotProduct / (magnitude1 * magnitude2);
    }
}
