using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace PdfEmbedding.Services
{
    public class StorageService
    {
        private readonly string _storagePath;

        public StorageService(string storagePath)
        {
            _storagePath = storagePath;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        // Save text chunks and embeddings to a JSON file
        public void SaveVectors(string fileName, List<string> chunks, List<List<float>> vectors)
        {
            var data = new { Chunks = chunks, Embeddings = vectors };
            var filePath = Path.Combine(_storagePath, fileName);
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, json);
        }

        // Load text chunks and embeddings from a file
        public (List<string> Chunks, List<List<float>> Embeddings) LoadVectors(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);
                var chunks = data.Chunks.ToObject<List<string>>();
                var embeddings = data.Embeddings.ToObject<List<List<float>>>();
                return (chunks, embeddings);
            }
            return (new List<string>(), new List<List<float>>());
        }
    }
}
