namespace EVotingSystem.Models
{
    // Base class
    public abstract class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Password { get; set; }

        public byte[] KeySalt { get; set; }
        public string CertificatePath { get; set; }
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
