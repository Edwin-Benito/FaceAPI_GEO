// Controllers/StudentsController.cs
// Depende de: AppDbContext + IFaceDetectionService (inyectados por constructor)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRollCall.Api.Data;
using SmartRollCall.Api.Models;
using SmartRollCall.Api.DTOs;
using SmartRollCall.Api.Services;

namespace SmartRollCall.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext         _context;
        private readonly FaceApiService        _faceApiService;

        public StudentsController(AppDbContext context, FaceApiService faceApiService)
        {
            _context        = context;
            _faceApiService = faceApiService;
        }

        // GET api/Students — Lista todos los alumnos de SQLite
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
            => await _context.Students.ToListAsync();

        // POST api/Students/register — Body: StudentRegistrationDto
        // Flujo: Azure CreatePerson → Azure AddFace → Azure Train → SQLite Save
        [HttpPost("register")]
        public async Task<ActionResult<Student>> RegisterStudentWithFace(StudentRegistrationDto dto)
        {
            try
            {
                // 1. Crear persona en Azure Face API → devuelve personId (GUID)
                string azurePersonId = await _faceApiService.CreatePersonAsync(dto.Name);

                // 2. Limpiar prefijo Base64 del Frontend y subir foto a Azure
                string base64Data = dto.PhotoBase64.Contains(',')
                    ? dto.PhotoBase64.Split(',').Last()
                    : dto.PhotoBase64;
                var imageBytes = Convert.FromBase64String(base64Data);
                await _faceApiService.AddFaceAsync(azurePersonId, imageBytes);

                // 3. Entrenar el PersonGroup con el nuevo rostro
                await _faceApiService.TrainPersonGroupAsync();

                // 4. Persistir en SQLite con el enlace al ID de Azure
                var newStudent = new Student
                {
                    Name             = dto.Name,
                    FaceVectorJson   = azurePersonId  // Guardamos el person GUID de Azure
                };
                _context.Students.Add(newStudent);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Alumno registrado biométricamente con éxito", student = newStudent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar la biometría: {ex.Message}");
            }
        }
    }
}