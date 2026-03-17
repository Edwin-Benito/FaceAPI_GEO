namespace SmartRollCall.Api.Models
{
    public class ClassGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string BeaconUUID { get; set; } = string.Empty;
        
        // Coordenadas para el Geofencing
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}