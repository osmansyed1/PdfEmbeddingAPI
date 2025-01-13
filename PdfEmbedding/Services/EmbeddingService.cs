using OpenAI.Embeddings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfEmbedding.Services
{
    public class EmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        public EmbeddingService(string apiKey)
        {
            _embeddingClient = new EmbeddingClient("text-embedding-3-small", apiKey);
        }

        // Generate embeddings for each chunk of text
        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddingsList = new List<List<float>>();

            foreach (var text in texts)
            {
                // Generate embedding for each chunk of text
                var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
                var vector = embedding.Value.ToFloats();
                embeddingsList.Add(new List<float>(vector.ToArray()));
            }

            return embeddingsList;
        }

        // Generate embedding for a single query
        public async Task<List<float>> GenerateQueryEmbeddingAsync(string query)
        {
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(query);
            var vector = embedding.Value.ToFloats();
            return new List<float>(vector.ToArray());
        }
    }
}
