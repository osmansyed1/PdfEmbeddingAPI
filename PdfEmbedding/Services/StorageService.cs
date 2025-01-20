using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PdfEmbedding.Services
{
    public class StorageService
    {
        private readonly string _storagePath;

        // Constructor to initialize the storage path
        public StorageService(string storagePath)
        {
            _storagePath = storagePath;
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        // Save or append text chunks and embeddings to a single JSON file (ensuring both arrays stay in sync)
        public void SaveVectors(string fileName, List<string> chunks, List<List<float>> vectors)
        {
            var filePath = Path.Combine(_storagePath, fileName);

            // Load existing data from the file if it exists
            List<string> existingChunks = new List<string>();
            List<List<float>> existingEmbeddings = new List<List<float>>();

            if (File.Exists(filePath))
            {
                var existingData = LoadVectors(fileName);
                existingChunks = existingData.Chunks;
                existingEmbeddings = existingData.Embeddings;
            }

            // Ensure that both chunks and vectors are appended correctly, maintaining order
            if (chunks.Count != vectors.Count)
                throw new InvalidOperationException("The number of chunks and embeddings must be the same");

            // Append the new chunks and embeddings to the existing ones
            existingChunks.AddRange(chunks);
            existingEmbeddings.AddRange(vectors);

            // Save the updated data back to the file
            var data = new { Chunks = existingChunks, Embeddings = existingEmbeddings };
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // Load text chunks and embeddings from the vector database (JSON file)
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
