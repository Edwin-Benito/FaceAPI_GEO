namespace SmartRollCall.Api.DTOs
{
    public class StudentRegistrationDto
    {
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string PhotoBase64 { get; set; } = string.Empty;
    }
}