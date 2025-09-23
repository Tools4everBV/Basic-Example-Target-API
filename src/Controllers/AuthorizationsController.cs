using EXAMPLE.API.Data;
using EXAMPLE.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EXAMPLE.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/authorizations")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "4-Authorizations")]
    public class AuthorizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthorizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/authorization
        /// <summary>
        /// Get all authorizations for all users
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// We need to retrieve information about all authorizations to support our import entitlement feature, enable reconciliation, and ultimately ensure proper governance.
        /// </remarks>
        /// <returns>List of all authorizations</returns>
        /// <response code="200">Returns the list of all authorizations</response>
        /// <response code="401">Authentication required or failed.</response>
        [HttpGet("", Name = "GetAllAuthorizations")]
        [ProducesResponseType(typeof(IEnumerable<Authorization>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<Authorization>>> GetAllAuthorizations()
        {
            var authorizations = await _context.Authorization.ToListAsync();
            return Ok(authorizations);
        }

        // GET: api/authorization/user/{userId}
        /// <summary>
        /// Get all authorizations for a specific user
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This returns all authorizations currently assigned to the given user.
        /// <br></br>
        /// Useful when validating which permissions/roles the user has at a given moment.
        /// </remarks>
        /// <response code="200">Returns a list of authorizations</response>
        /// <response code="404">If the user has no authorizations</response>
        /// <response code="401">Authentication required or failed.</response>
        [HttpGet("user/{userId:int}", Name = "GetAuthorizationsByUserId")]
        [ProducesResponseType(typeof(IEnumerable<Authorization>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<IEnumerable<Authorization>>> GetAuthorizationsByUserId(int userId)
        {
            var userExists = await _context.User.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"User with id {userId} not found.");
            }

            var authorizations = await _context.Authorization.Where(a => a.UserId == userId).ToListAsync();
            return Ok(authorizations);
        }

        // POST: api/authorization
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Add a new authorization for a user
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This action is used when an authorization is granted to a user.  
        /// If an identical authorization already exists for the given <c>userId</c> and <c>roleId</c>,  
        /// the existing record will be returned with a <c>200 OK</c> response instead of creating a duplicate.  
        /// <br/><br/>
        /// Example request:
        ///   
        ///     POST /authorization
        ///     {
        ///        "roleId": 1,
        ///        "userId": 1
        ///     }
        /// </remarks>
        /// <param name="auth">The authorization object to add.</param>
        /// <response code="201">The authorization was successfully created.</response>
        /// <response code="200">The authorization already existed and is returned.</response>
        /// <response code="400">Invalid request payload.</response>
        /// <response code="401">Authentication required or failed.</response>
        [HttpPost("", Name = "AddAuthorization")]
        [ProducesResponseType(typeof(Authorization), 201)]
        [ProducesResponseType(typeof(Authorization), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<Authorization>> PostAuthorization([FromBody] Authorization auth)
        {
            var existingAuth = await _context.Authorization.FirstOrDefaultAsync(a => a.UserId == auth.UserId && a.RoleId == auth.RoleId);
            if (existingAuth != null)
            {
                return Ok(existingAuth);
            }

            _context.Authorization.Add(auth);
            await _context.SaveChangesAsync();

            return new ObjectResult(auth) { StatusCode = StatusCodes.Status201Created };
        }

        // DELETE: api/authorization/:id
        /// <summary>
        /// Delete authorization (by Id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// We will use this action when an authorization is revoked from a user. This action does not require a response. A [204 No Content] is sufficient.
        /// </remarks>
        /// <param name="auth">The authorization that will be removed.</param>
        /// <response code="204">The authorization was successfully removed.</response>
        /// <response code="404">No authorization with the specified Id was found.</response>
        /// <response code="401">Authentication required or failed.</response>
        [HttpDelete("{id}", Name = "DeleteAuthorization")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAuthorization(int id)
        {
            var auth = await _context.Authorization.FindAsync(id);
            if (auth == null)
            {
                return NotFound();
            }

            _context.Authorization.Remove(auth);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
