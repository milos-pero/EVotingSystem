namespace EVotingSystem.Models
{
    // Base class
    public abstract class User
    {
        public int LoginAttempts { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PasswordHash { get; set; }

        public byte[] KeySalt { get; set; }
        public string CertificatePath { get; set; }
        public string PublicCertPath { get; set; }
}

    public class Organizer : User
    {
        public string OrganizationName { get; set; }
        public string OrganizationId { get; set; }
    }
    public class Voter : User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
    }
}
