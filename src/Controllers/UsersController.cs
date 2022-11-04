using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAMPLE.API.Data;
using EXAMPLE.API.Data.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace EXAMPLE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        // Returns a list of all users.
        // This call is not required for HelloID, it will be deleted from the swagger.yaml.
        // <summary>
        /// Get users
        /// </summary>
        [HttpGet(Name = "GetAllUsers")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.User.ToListAsync();
        }

        // GET: api/Users/:emloyeeId
        /// <summary>
        /// Get user (by EmployeeId)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Before we add a user account to the target application, we need to validate if that user account exists.
        /// <br></br>
        /// Initially, the internal database ID is not known to us. Therefore, we prefer to validate this using the `EmployeeId` since this is unique and available in HelloID.
        /// <br></br>
        /// We only need to retrieve the user account using the `employeeId` in our initial create event. For all other events we will use the internal database ID. The `database ID` is the also key we correlate on in our create event.
        /// </remarks>
        /// <param name="employeeId"></param>
        /// <response code="200"></response>
        [HttpGet("ByEmployeeId/{employeeId}", Name = "GetUserByEmployeeId")]
        [ActionName(nameof(GetUserByEmployeeId))]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> GetUserByEmployeeId(string employeeId)
        {
            var users = await _context.User.ToListAsync();
            var user = users.SingleOrDefault(u => u.EmployeeId == employeeId);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/Users/:id
        /// <summary>
        /// Get user (by id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Before we update a particular user account, we need to validate if that user account still exists.
        /// <br></br>
        /// Validating if the user account exists is an integral part in all our lifecycle events because the user account might be <em>unintentionally</em> deleted. In which case the lifecycle event will fail. 
        /// For example: when we want to enable the user account on the day the contract takes effect.
        /// <br></br>
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="200"></response>
        [HttpGet("{id:int}", Name = "GetUserById")]
        [ActionName(nameof(GetUserById))]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PATCH: api/Users/:id
        /// <summary>
        /// Update user (by id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// To update a user account we prefer to see an update call in the form of a patch. Which means that we only update the values that have been changed.
        /// <br></br>
        /// <em>In this case the patch method is implemented using <a href="https://jsonpatch.com/">JSON Patch</a>. Note that this might not have to be the best solution for your application.</em>
        /// <br></br>
        /// Example:
        ///   
        ///     PATCH /Users/:id
        ///     [  
        ///         {
        ///             "op": "replace",
        ///             "path": "lastName",
        ///             "value": "Jane"
        ///         }
        ///     ]
        ///     
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <response code="200"></response>
        [HttpPatch("{id}", Name = "PatchUser")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Add user
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// Adds a new user account to the target system. The response must contain the internal database ID since this is the key we correlate on and will be used for consecutive requests to the target system. Therefore, the `id` field is enlisted in the user schema.
        /// <br></br>
        /// Example:
        ///     
        ///     POST /Users
        ///     {
        ///        "employeeId": "1000000",
        ///        "firstName": "John",
        ///        "lastName": "Doe",
        ///        "email: "JDoe@enyoi",
        ///        "active": "false"
        ///     }
        ///     
        /// </remarks>
        /// <param name="User"></param>
        /// <response code="201"></response>
        [HttpPost(Name = "AddUser")]
        [ProducesResponseType(typeof(User), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<User>> PostUser(User User)
        {
            _context.User.Add(User);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { Id = User.Id }, User);
        }

        // POST: api/Users/:id/Authorizations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Add authorization
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// We will use this action when an authorization is granted to a user. Since we do not store the result in HelloID, this action does not require a response
        /// <br></br>
        /// Example:
        ///   
        ///     POST /Users/:id/Authorizations
        ///     {
        ///        "roleId": 1,
        ///        "userId": 1
        ///     }
        ///     
        /// </remarks>
        /// <param name="auth">The authorization that will be added.</param>
        /// <response code="201"></response>
        [HttpPost("Authorizations/Add", Name = "AddAuthorization")]
        [ProducesResponseType(typeof(Authorization), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Authorization>> PostAuthorization([FromBody] Authorization auth)
        {
            _context.Authorization.Add(auth);
            await _context.SaveChangesAsync();

            return new ObjectResult(auth) { StatusCode = StatusCodes.Status201Created };
        }

        // DELETE: api/Users/:id
        /// <summary>
        /// Delete authorization
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// We will use this action when an authorization is revoked from a user. This action does not require a response. A [204 No Content] is sufficient.
        /// </remarks>
        /// <param name="auth">The authorization that will be removed.</param>
        /// <response code="204"></response>
        [HttpDelete("Authorizations/Delete", Name = "DeleteAuthorization")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAuthorization(int userId, int roleId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userAuths = await _context.Authorization.Where(a => a.UserId == user.Id).ToListAsync();
            var authToRemove = userAuths.Where(a => a.RoleId == roleId).SingleOrDefault();
            _context.Authorization.Remove(authToRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/:id
        /// <summary>
        /// Delete user (by id)
        /// </summary>
        /// <remarks>
        /// <h2>Implementation notes</h2>
        /// This action does not require a response. A [204 No Content] is sufficient.
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="204"></response>
        [HttpDelete("{id}", Name = "DeleteUser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
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