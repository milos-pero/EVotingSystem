using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EVotingSystem.Models;

namespace EVotingSystem.Security
{
    public static class CertificateService
    {
        public static X509Certificate2 GenerateCertificate(User user, string password)
        {
            // 1️⃣ Generate RSA key pair
            using RSA rsa = RSA.Create(2048);

            // 2️⃣ Build subject name
            string subject = BuildSubject(user);

            var request = new CertificateRequest(
                new X500DistinguishedName(subject),
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            // 3️⃣ Add extensions
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                    critical: true));

            // Custom extension to store role
            request.CertificateExtensions.Add(
                new X509Extension(
                    "1.2.3.4.5.6.7.8.1", // custom OID
                    System.Text.Encoding.UTF8.GetBytes(user.GetType().Name),
                    false));

            // 4️⃣ Create self-signed certificate (TEMP)
            var cert = request.CreateSelfSigned(
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(2)
            );

            // 5️⃣ Export as PFX (protected private key)
            byte[] pfxData = cert.Export(X509ContentType.Pfx, password);

            string fileName = $"{Guid.NewGuid()}.pfx";
            string certDir = Path.Combine(AppContext.BaseDirectory, "Certificates");

            if (!Directory.Exists(certDir))
            {
                Directory.CreateDirectory(certDir);
            }

            string filePath = Path.Combine(certDir, fileName);
            File.WriteAllBytes(filePath, pfxData);


            return new X509Certificate2(
                pfxData,
                password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
            );
        }

        private static string BuildSubject(User user)
        {
            if (user is Organizer organizer)
            {
                return $"CN={organizer.OrganizationName}, O={organizer.OrganizationName}, SERIALNUMBER={organizer.OrganizationId}";
            }
            else if (user is Voter voter)
            {
                return $"CN={voter.Username}, GIVENNAME={voter.FirstName}, SURNAME={voter.LastName}";
            }

            throw new InvalidOperationException("Unknown user type");
        }
    }
}
