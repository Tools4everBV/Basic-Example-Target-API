using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAMPLE.API.Data;
using EXAMPLE.API.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace EXAMPLE.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "3-Roles")]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roles
        /// <summary>
        /// Get all roles
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Before we can assign a role to a user, we first need to retrieve all available roles. Then, we build out our business rules to, ultimately grant authorizations to users based on information coming from an HR source.
        /// </remarks>
        /// <response code="200">A list of roles was successfully retrieved.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpGet(Name = "GetAllRoles")]
        [ProducesResponseType(typeof(List<Role>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<Role>>> GetRole()
        {
            return await _context.Role.ToListAsync();
        }
    }
}
