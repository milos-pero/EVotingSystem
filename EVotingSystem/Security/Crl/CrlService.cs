using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace EVotingSystem.Security.Crl
{
    public static class CrlService
    {
        private static string GetCrlPath(X509Certificate2 cert)
        {
            if (cert.Issuer.Contains("Organizer"))
                return "Security/Crl/OrganizerCA_crl.json";

            if (cert.Issuer.Contains("Voter"))
                return "Security/Crl/VoterCA_crl.json";

            throw new InvalidOperationException("Unknown CA issuer.");
        }

        public static bool IsRevoked(X509Certificate2 cert)
        {
            var path = GetCrlPath(cert);

            if (!File.Exists(path))
                return false;

            var list = JsonSerializer.Deserialize<List<RevokedCertificate>>(
                File.ReadAllText(path)) ?? new();

            return list.Any(r => r.SerialNumber == cert.SerialNumber);
        }

        public static void Revoke(X509Certificate2 cert, string reason)
        {
            var path = GetCrlPath(cert);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var list = File.Exists(path)
                ? JsonSerializer.Deserialize<List<RevokedCertificate>>(File.ReadAllText(path))!
                : new List<RevokedCertificate>();

            if (list.Any(r => r.SerialNumber == cert.SerialNumber))
                return;

            list.Add(new RevokedCertificate
            {
                SerialNumber = cert.SerialNumber,
                RevokedAt = DateTime.Now,
                Reason = reason
            });

            File.WriteAllText(path,
                JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
