namespace EXAMPLE.API.Data.Models
{
    /// <summary>
    /// The roles that are available in the target system. Like; 'HelpDesk' or 'Admin'.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// This is the internal / database Id.
        /// <br>
        /// Typically this value will be set by the application itself.
        /// </br>
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The DisplayName of the role
        /// </summary>
        /// <example>Admin</example>
        public string DisplayName { get; set; }
    }
}
