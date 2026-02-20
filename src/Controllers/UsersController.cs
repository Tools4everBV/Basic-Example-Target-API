using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAMPLE.API.Data;
using EXAMPLE.API.Data.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Filters;

namespace EXAMPLE.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "2-Users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        /// <summary>
        /// Retrieve all users
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This endpoint returns all users in the system.  
        /// It is primarily used to support the import entitlement feature, enable reconciliation,
        /// and ensure proper governance across connected systems.
        /// </remarks>
        /// <response code="200">A list of users was successfully retrieved.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpGet(Name = "ListUsers")]
        [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.User.ToListAsync();
        }

        // GET: api/users/{employeeId}
        /// <summary>
        /// Retrieve a user (by employeeId)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Before we add a user account to the target application, we must validate whether that user already exists.
        /// Since the internal database ID is not known at the time of the initial create event,
        /// we use the <c>employeeId</c> (which is unique and provided by HelloID) for validation.
        /// <br/>
        /// Once the user account is created and correlated, subsequent events will use the internal database ID for lookups.
        /// </remarks>
        /// <param name="employeeId">The employee ID of the user to look up.</param>
        /// <response code="200">The user was found and returned.</response>
        /// <response code="404">No user with the specified employee ID was found.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpGet("employeeid/{employeeid}", Name = "GetUserByEmployeeId")]
        [ActionName(nameof(GetUserByEmployeeId))]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<User>> GetUserByEmployeeId(string employeeid)
        {
            var users = await _context.User.ToListAsync();
            var user = users.SingleOrDefault(u => u.EmployeeId == employeeid);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/users/:id
        /// <summary>
        /// Get user (by Id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Before we update a particular user account, we need to validate if that user account still exists.
        /// <br/>
        /// Validating if the user account exists is an integral part in all our lifecycle events because the user 
        /// account might be <em>unintentionally</em> delete, in which case the lifecycle event will fail.
        /// For example: when we want to enable the user account on the day the contract takes effect.
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="200">The user was found and returned.</response>
        /// <response code="404">No user with the specified  was found.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpGet("{id:int}", Name = "GetUserById")]
        [ActionName(nameof(GetUserById))]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PATCH: api/users/{id}
        /// <summary>
        /// Partially update a user (by Id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This endpoint supports partial updates to a user account using the HTTP PATCH method.
        /// Only the specified fields in the patch document will be updated.
        /// <br/>
        /// <em>This implementation uses <a href="https://jsonpatch.com/">JSON Patch</a>,
        /// which allows operations like <c>replace</c>, <c>add</c>, and <c>remove</c>.
        /// Consider whether JSON Patch is the best solution for your application before using it.</em>
        /// <br/>
        /// Example request:
        /// 
        ///     PATCH /user/{id}
        ///     [  
        ///         {
        ///             "op": "replace",
        ///             "path": "active",
        ///             "value": "false"
        ///         }
        ///     ]
        /// 
        /// </remarks>
        /// <param name="id">The internal database ID of the user to update.</param>
        /// <param name="patchDoc">The JSON Patch document describing the updates to apply.</param>
        /// <response code="200">The user was successfully updated and returned.</response>
        /// <response code="400">The patch document was invalid.</response>
        /// <response code="404">No user with the specified ID was found.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpPatch("{id}", Name = "PatchUser")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [Consumes("application/json-patch+json")]
        [SwaggerRequestExample(typeof(JsonPatchDocument<User>), typeof(UserPatchExample))]
        public async Task<IActionResult> PatchUser(int id, [FromBody] JsonPatchDocument<User> patchDoc)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            patchDoc.ApplyTo(entity, ModelState);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }

        // POST: api/users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This endpoint creates a new user account in the target system.
        /// The response must include the internal database <c>id</c> because this is the key used for correlation
        /// and will be required for subsequent update, patch, and delete operations.
        /// <br/>
        /// Example request:
        /// 
        ///     POST /user
        ///     {
        ///        "employeeId": "1000000",
        ///        "firstName": "John",
        ///        "lastName": "Doe",
        ///        "email": "JDoe@enyoi",
        ///        "active": false
        ///     }
        /// 
        /// </remarks>
        /// <param name="User">The user object to create.</param>
        /// <response code="201">The user was successfully created.</response>
        /// <response code="400">Invalid request payload.</response>
        /// <response code="401">Authentication failed or was not provided.</response>
        [HttpPost(Name = "AddUser")]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<User>> PostUser(User User)
        {
            try
            {
                _context.User.Add(User);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetUserById), new { Id = User.Id }, User);
            }
            catch (DbUpdateException)
            {
                return BadRequest($"EmployeeId '{User.EmployeeId}' already exists.");
            }
        }

        // DELETE: api/users/:id
        /// <summary>
        /// Delete user (by Id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This action does not require a response. A [204 No Content] is sufficient.
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="204">The user was successfully removed.</response>
        /// <response code="404">No user with the specified Id was found.</response>
        /// <response code="401">Authentication required or failed.</response>
        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

public class UserPatchExample : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new[]
        {
            new { op = "replace", path = "active", value = false }
        };
    }
}