using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRollCall.Api.Data;
using SmartRollCall.Api.Models;

namespace SmartRollCall.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClassGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ClassGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassGroup>>> GetClassGroups()
        {
            return await _context.ClassGroups.ToListAsync();
        }
    }
}