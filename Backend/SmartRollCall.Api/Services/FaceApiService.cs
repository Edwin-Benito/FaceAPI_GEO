using System.Net.Http.Headers;
using System.Text.Json;
using System.Net.Http.Json;

namespace SmartRollCall.Api.Services
{
    public class FaceApiService : IFaceDetectionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _personGroupId;

        public FaceApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _personGroupId = config["AzureFaceApi:PersonGroupId"] ?? "tiid_02_05";

            var endpoint = config["AzureFaceApi:Endpoint"] ?? "";
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                _httpClient.BaseAddress = new Uri(endpoint);
            }

            var key = config["AzureFaceApi:SubscriptionKey"] ?? "";
            if (!string.IsNullOrWhiteSpace(key))
            {
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            }
        }

        public async Task<bool> CreatePersonGroupAsync()
        {
            var response = await _httpClient.PutAsJsonAsync($"face/v1.0/persongroups/{_personGroupId}", new { name = "Grupo TIID_02_05", recognitionModel = "recognition_04" });
            return response.IsSuccessStatusCode;
        }

        public async Task<string> CreatePersonAsync(string name)
        {
            var response = await _httpClient.PostAsJsonAsync($"face/v1.0/persongroups/{_personGroupId}/persons", new { name = name });
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            return result.GetProperty("personId").GetString() ?? "";
        }

        public async Task<bool> AddFaceAsync(string personId, byte[] imageBytes)
        {
            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await _httpClient.PostAsync($"face/v1.0/persongroups/{_personGroupId}/persons/{personId}/persistedFaces?detectionModel=detection_03", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TrainPersonGroupAsync()
        {
            var response = await _httpClient.PostAsync($"face/v1.0/persongroups/{_personGroupId}/train", null);
            return response.IsSuccessStatusCode;
        }

        // Implementación requerida por IFaceDetectionService
        // Llama a Azure Face Detect y retorna true si hay al menos un rostro
        public async Task<bool> IsFacePresentAsync(string imageBase64)
        {
            try
            {
                var imageBytes = Convert.FromBase64String(
                    imageBase64.Contains(',') ? imageBase64.Split(',').Last() : imageBase64);
                using var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                var res = await _httpClient.PostAsync(
                    "face/v1.0/detect?returnFaceId=false&detectionModel=detection_03", content);
                if (!res.IsSuccessStatusCode) return false;
                var data = await res.Content.ReadFromJsonAsync<List<JsonElement>>();
                return data != null && data.Count > 0;
            }
            catch { return false; }
        }

        public async Task<(string? PersonId, double Confidence)> IdentifyFaceAsync(byte[] imageBytes)
        {
            try
            {
                using var content = new ByteArrayContent(imageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                
                var detectRes = await _httpClient.PostAsync("face/v1.0/detect?returnFaceId=true&recognitionModel=recognition_04&detectionModel=detection_03", content);
                if (!detectRes.IsSuccessStatusCode) return (null, 0);
                
                var detectData = await detectRes.Content.ReadFromJsonAsync<List<JsonElement>>();
                if (detectData == null || detectData.Count == 0) return (null, 0);
                
                string faceId = detectData[0].GetProperty("faceId").GetString()!;

                var identifyRes = await _httpClient.PostAsJsonAsync("face/v1.0/identify", new {
                    personGroupId = _personGroupId,
                    faceIds = new[] { faceId },
                    maxNumOfCandidatesReturned = 1
                });

                if (!identifyRes.IsSuccessStatusCode) return (null, 0);
                var identifyData = await identifyRes.Content.ReadFromJsonAsync<List<JsonElement>>();
                
                if (identifyData != null && identifyData.Count > 0)
                {
                    var candidates = identifyData[0].GetProperty("candidates");
                    if (candidates.GetArrayLength() > 0)
                    {
                        var top = candidates[0];
                        return (top.GetProperty("personId").GetString(), top.GetProperty("confidence").GetDouble());
                    }
                }
            }
            catch { }
            return (null, 0);
        }
    }
}