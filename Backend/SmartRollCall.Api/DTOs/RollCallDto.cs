namespace SmartRollCall.Api.DTOs
{
    public class RollCallDto
    {
        public int ClassGroupId { get; set; } // El ID del grupo (Ej. TIID_02_05)
        public string PhotoBase64 { get; set; } = string.Empty; // La foto del salón
        public double Latitude { get; set; } // GPS del celular del profesor
        public double Longitude { get; set; }
        public int BleRssi { get; set; } // Fuerza de la señal Bluetooth del ESP32
    }
}