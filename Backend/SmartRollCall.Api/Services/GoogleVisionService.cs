using Google.Cloud.Vision.V1;

namespace SmartRollCall.Api.Services
{
    public class GoogleVisionService : IFaceDetectionService
    {
        private readonly ImageAnnotatorClient _client;

        public GoogleVisionService(IConfiguration config)
        {
            // Obtenemos la llave del appsettings.json
            string apiKey = config["GoogleCloud:ApiKey"] ?? "";

            // Configuramos el cliente para usar la API Key directamente
            var builder = new ImageAnnotatorClientBuilder
            {
                ApiKey = apiKey
            };

            _client = builder.Build();
        }

        public async Task<bool> IsFacePresentAsync(string imageBase64)
        {
            try
            {
                // Limpiar prefijo data:image/jpeg;base64, si existe
                var base64Data = imageBase64.Contains(',') ? imageBase64.Split(',').Last() : imageBase64;
                
                // Convertimos la imagen de Base64 a bytes para Google
                var imageBytes = Convert.FromBase64String(base64Data);
                var image = Image.FromBytes(imageBytes);

                // Llamamos a la detección de rostros
                var response = await _client.DetectFacesAsync(image);

                // Si detecta al menos un rostro, devolvemos true
                return response.Count > 0;
            }
            catch (Exception ex)
            {
                // Si hay un error (ej. Key inválida), lo vemos en la consola
                Console.WriteLine($"Error de Google Vision: {ex.Message}");
                return false;
            }
        }
    }
}
