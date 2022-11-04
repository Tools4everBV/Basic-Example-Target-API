using System.ComponentModel.DataAnnotations;

namespace EXAMPLE.API.Data.Models
{
    /// <summary>
    /// The user schema contains all the parameters we expect to be present in an application.
    /// <br>
    /// Your application / user schema might have different names for parameters, or far more parameters than enlisted in this schema.
    /// For example: A 'PhoneNumber' for two-factor authentication or a more complex multi-layered schema.
    /// </br>
    /// <br>
    /// This schema contains the bare minimum we need in order to build a solid connector.
    /// </br>
    /// </summary>
    public class User
    {
        /// <summary>
        /// This is the internal / database Id.
        /// <br>
        /// Typically this value will be set by the application itself.
        /// </br>
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The EmployeeId or ExternalId of the user.
        /// </summary>
        /// <example>100000</example>
        [Required(ErrorMessage = "EmployeeId is required")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// The firstName of the user.
        /// </summary>
        /// <example>John</example>
        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// The lastName of the user.
        /// </summary>
        /// <example>Doe</example>
        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        /// <example>J.Doe@enyoi</example>
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        /// <summary>
        /// Defines if the user is active or not. We will update this value when a user is enabled or disabled.
        /// <br>
        /// <h3>Remarks</h3>
        /// Defines if the user is active or not. We will update this value when a user is enabled or disabled. 
        /// When we initially create a user, we prefer to create that user in a `disabled state`. On the day the contract takes effect the user account will be enabled.
        /// </br>
        /// </summary>
        /// <example>False</example>
        [Required(ErrorMessage = "Active is required")]
        public bool? Active { get; set; }
    }
}
