using Microsoft.AspNetCore.Mvc;
using PdfEmbedding.Models;
using PdfEmbedding.Services;

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

        // Dynamically chunk the text based on the size of the content
        var chunks = _pdfProcessingService.ChunkText(text, 500); // 500 characters per chunk

        // Generate embeddings for each chunk
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks);

        // Save the embeddings to storage (in Vectors folder)
        _storageService.SaveVectors(file.FileName + ".json", embeddings);

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
            // 1. Load embeddings stored on disk (each chunk will have its own embedding)
            var embeddings = _storageService.LoadVectors(request.FileName);

            if (!embeddings.Any())
                return BadRequest("No embeddings available to search");

            // 2. Generate embedding for the question
            var queryEmbedding = await _embeddingService.GenerateQueryEmbeddingAsync(request.Question);

            // 3. Find the top matching embeddings based on cosine similarity (you can change the number of top matches)
            var topMatches = FindTopMatches(queryEmbedding, embeddings, 3); // Adjust "3" to get more matches

            // 4. Generate an answer based on the top matching chunks (context) using LLM (OpenAI)
            var context = string.Join("\n", topMatches.Select(match => $"Chunk {match.Index + 1}: {embeddings[match.Index]}"));

            // Here you may provide the "context" from the top chunks to the OpenAI model
            var generatedAnswer = await _openAIService.GenerateAnswer(request.Question, context);

            // 5. Return the generated answer along with the similarity score (optional)
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
    private List<(int Index, float Similarity)> FindTopMatches(
        List<float> queryEmbedding,
        List<List<float>> embeddings,
        int topK)
    {
        var similarities = new List<(int Index, float Similarity)>();

        // Calculate cosine similarity for each embedding
        for (int i = 0; i < embeddings.Count; i++)
        {
            float similarity = CalculateCosineSimilarity(queryEmbedding, embeddings[i]);
            similarities.Add((i, similarity));
        }

        // Return the top K most similar matches
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

        // Calculate dot product and magnitudes
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
