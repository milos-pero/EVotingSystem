namespace EVotingSystem.Models
{
    // Base class
    public abstract class User
    {
        public string Password { get; set; } // All users have a password

        // You can add common methods later, like VerifyPassword()
    }

    // Organizer subclass
    public class Organizer : User
    {
        public string OrganizationName { get; set; }
        public string OrganizationId { get; set; }

        // Organizer-specific methods
        // public void CreateElection() { ... }
    }

    // Voter subclass
    public class Voter : User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        // Voter-specific methods
        // public void Vote() { ... }
    }
}
