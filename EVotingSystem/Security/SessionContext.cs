using EVotingSystem.Models;
using System.Security.Cryptography.X509Certificates;

namespace EVotingSystem.Security
{
    public static class SessionContext
    {
        public static User? CurrentUser { get; set; }
        public static X509Certificate2? UserCertificate { get; set; }
    }
}
