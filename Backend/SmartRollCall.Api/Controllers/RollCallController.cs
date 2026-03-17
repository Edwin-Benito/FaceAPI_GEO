using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRollCall.Api.Data;
using SmartRollCall.Api.Models;
using SmartRollCall.Api.Services;

namespace SmartRollCall.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RollCallController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFaceDetectionService _faceService; // Cambio aquí

        public RollCallController(AppDbContext context, IFaceDetectionService faceService)
        {
            _context = context;
            _faceService = faceService;
        }

        // GET api/RollCall/history — Historial de asistencias para la tabla del frontend
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var records = await _context.AttendanceRecords
                .Include(r => r.Student)
                .Include(r => r.ClassGroup)
                .OrderByDescending(r => r.Timestamp)
                .Take(50)
                .Select(r => new {
                    id       = r.Id,
                    usuario  = r.Student != null ? r.Student.Name : "Desconocido",
                    hora     = r.Timestamp.ToString("HH:mm:ss"),
                    aula     = r.ClassGroup != null ? r.ClassGroup.GroupName : "?",
                    status   = r.IsValidLocation ? "AUTHORIZED" : "DENIED"
                })
                .ToListAsync();
            return Ok(records);
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] RollCallRequest req)
        {
            var aula = await _context.ClassGroups.FindAsync(req.ClassGroupId);
            if (aula == null) return NotFound(new { message = "Aula no encontrada." });

            double dist = CalculateDistance(req.Latitude, req.Longitude, aula.Latitude, aula.Longitude);
            if (dist > 15) return BadRequest(new { message = "Fuera de rango GPS.", distancia = Math.Round(dist, 2) });

            // Validación de rostro simple
            bool facePresent = await _faceService.IsFacePresentAsync(req.PhotoBase64);
            if (!facePresent) return Unauthorized(new { message = "No se detectó un rostro humano." });

            var record = new AttendanceRecord {
                StudentId = 1, // ID de Edwin en tu DB
                ClassGroupId = aula.Id,
                Timestamp = DateTime.Now,
                IsValidLocation = true
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asistencia Exitosa", distancia = Math.Round(dist, 2) });
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2) {
            double R = 6371000;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) + Math.Cos(lat1 * Math.PI/180) * Math.Cos(lat2 * Math.PI/180) * Math.Sin(dLon/2) * Math.Sin(dLon/2);
            return R * (2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)));
        }
    }

    public class RollCallRequest {
        public int ClassGroupId { get; set; }
        public string PhotoBase64 { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}