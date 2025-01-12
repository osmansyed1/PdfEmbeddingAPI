using OpenAI.Embeddings;

namespace PdfEmbedding.Services
{
    public class EmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;

        public EmbeddingService(string apiKey)
        {
            _embeddingClient = new EmbeddingClient("text-embedding-3-small", apiKey);
        }

        public async Task<List<List<float>>> GenerateEmbeddingsAsync(List<string> texts)
        {
            var embeddingsList = new List<List<float>>();

            foreach (var text in texts)
            {
                var embedding = await _embeddingClient.GenerateEmbeddingAsync(text);
                var vector = embedding.Value.ToFloats();
                embeddingsList.Add(new List<float>(vector.ToArray()));
            }

            return embeddingsList;
        }

        public async Task<List<float>> GenerateQueryEmbeddingAsync(string query)
        {
            var embedding = await _embeddingClient.GenerateEmbeddingAsync(query);
            var vector = embedding.Value.ToFloats();
            return new List<float>(vector.ToArray());
        }
    }
}
