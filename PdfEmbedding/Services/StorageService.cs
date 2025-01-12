/*using Newtonsoft.Json;

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

        public void SaveVectors(string fileName, List<List<float>> vectors)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            var json = JsonConvert.SerializeObject(vectors);
            File.WriteAllText(filePath, json);
        }

        public List<List<float>> LoadVectors(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<List<float>>>(json)!;
            }
            return new List<List<float>>();
        }
    }
}
*/




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

        // Save only embeddings (vectors) to a JSON file
        public void SaveVectors(string fileName, List<List<float>> vectors)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            var json = JsonConvert.SerializeObject(vectors);
            File.WriteAllText(filePath, json);
        }

        // Load only embeddings (vectors) from a file
        public List<List<float>> LoadVectors(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<List<float>>>(json)!;
            }
            return new List<List<float>>();
        }
    }
}
