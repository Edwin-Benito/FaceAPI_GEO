using System;

namespace SmartRollCall.Api.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int ClassGroupId { get; set; }
        public ClassGroup? ClassGroup { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsValidLocation { get; set; } // True si el GPS y el BLE coincidieron
    }
}