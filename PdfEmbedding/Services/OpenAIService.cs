using Newtonsoft.Json;
using System.Text;

namespace PdfEmbedding.Services
{
    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public OpenAIService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<string> GenerateAnswer(string question, string context)
        {
            var url = "https://api.openai.com/v1/chat/completions";

            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new
                {
                    role = "system",
                    content = "You are a helpful assistant. Using the provided context, answer the user's question concisely and accurately. If the context doesn't contain enough information to answer the question, say so."
                },
                new
                {
                    role = "user",
                    content = $"Question: {question}\n\nContext: {context}\n\nProvide a concise answer based on this context."
                }
            },
                temperature = 0.3,
                max_tokens = 150
            };

            var response = await _httpClient.PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API error: {await response.Content.ReadAsStringAsync()}");

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return responseObject.choices[0].message.content.ToString();
        }
    }
}
