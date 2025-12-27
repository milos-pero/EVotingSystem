using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EVotingSystem.Models;
using EVotingSystem.Security.Ca;

namespace EVotingSystem.Security
{
    public static class CertificateIssuer
    {
        public static X509Certificate2 IssueUserCertificate(User user)
        {
            using RSA rsa = RSA.Create(2048);

            string subject = user switch
            {
                Organizer o => $"CN={o.OrganizationName}, SERIALNUMBER={o.OrganizationId}",
                Voter v => $"CN={v.Username}",
                _ => throw new InvalidOperationException()
            };

            var request = new CertificateRequest(
                subject,
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                    true));

            // Role extension
            request.CertificateExtensions.Add(
                new X509Extension(
                    "1.2.3.4.5.6.7.8.1",
                    System.Text.Encoding.UTF8.GetBytes(user.GetType().Name),
                    false));

            var issuer = user is Organizer
                ? OrganizerCaService.GetOrCreateCa()
                : VoterCaService.GetOrCreateCa();

            var cert = request.Create(
                issuer,
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(2),
                Guid.NewGuid().ToByteArray());

            if (!issuer.HasPrivateKey)
            {
                throw new Exception("Issuer CA has no private key!");
            }

            return cert.CopyWithPrivateKey(rsa);
        }
    }
}
