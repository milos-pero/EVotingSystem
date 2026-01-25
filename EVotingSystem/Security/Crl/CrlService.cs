using EVotingSystem.Security.Ca;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EVotingSystem.Security.Crl
{
    public static class CrlService
    {
        private const string CrlFolder = "Certificates/CRL";

        private static string GetCrlPath(X509Certificate2 caCert)
        {
            Directory.CreateDirectory(CrlFolder);

            if (caCert.Subject.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(CrlFolder, "OrganizerCA.crl");

            if (caCert.Subject.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(CrlFolder, "VoterCA.crl");

            throw new InvalidOperationException("Unknown CA certificate.");
        }

        // Keep revoked serials in memory for simplicity
        private static readonly Dictionary<string, HashSet<string>> RevokedByCa = new();

        public static void RevokeCertificate(X509Certificate2 certToRevoke)
        {
            X509Certificate2 caCert;
            string caPassword;

            if (certToRevoke.Issuer.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
            {
                caCert = OrganizerCaService.GetOrCreateCa();
                caPassword = "organizer";
            }
            else if (certToRevoke.Issuer.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
            {
                caCert = VoterCaService.GetOrCreateCa();
                caPassword = "voter";
            }
            else
            {
                throw new InvalidOperationException("Cannot revoke certificate: unknown CA issuer.");
            }

            // Load the CA with private key (Exportable)
            var caCertWithKey = new X509Certificate2(
                Path.Combine("Certificates", Path.GetFileName(caCert.PfxPath())),
                caPassword,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
            );

            // Revoke the certificate
            RevokeCertificateInternal(caCertWithKey, certToRevoke);
        }

        private static void RevokeCertificateInternal(X509Certificate2 caCertWithKey, X509Certificate2 certToRevoke)
        {
            string caName = caCertWithKey.Subject;

            if (!RevokedByCa.ContainsKey(caName))
                RevokedByCa[caName] = new HashSet<string>();

            var revokedSerials = RevokedByCa[caName];
            revokedSerials.Add(certToRevoke.SerialNumber);

            GenerateCrl(caCertWithKey, revokedSerials);
        }

        private static void GenerateCrl(X509Certificate2 caCert, HashSet<string> revokedSerials)
        {
            string crlPath = GetCrlPath(caCert);

            using RSA rsa = caCert.GetRSAPrivateKey()!;

            // Create CRL in DER format manually
            using var crlStream = new MemoryStream();
            using var writer = new BinaryWriter(crlStream);

            // This is simplified — for production you should generate proper X509 CRL structure
            // For now, we just store serials in a simple format
            writer.Write(DateTime.UtcNow.ToBinary());
            writer.Write(revokedSerials.Count);
            foreach (var serial in revokedSerials)
            {
                writer.Write(serial);
            }

            writer.Flush();

            // Sign the data with RSA
            var data = crlStream.ToArray();
            var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Save CRL + signature to disk
            using var fs = new FileStream(crlPath, FileMode.Create, FileAccess.Write);
            fs.Write(data, 0, data.Length);
            fs.Write(signature, 0, signature.Length);
        }

        public static bool IsRevoked(X509Certificate2 cert)
        {
            X509Certificate2 caCert;

            if (cert.Issuer.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
                caCert = OrganizerCaService.GetOrCreateCa();
            else if (cert.Issuer.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
                caCert = VoterCaService.GetOrCreateCa();
            else
                throw new InvalidOperationException("Unknown CA issuer");

            return IsRevoked(caCert, cert);
        }

        public static bool IsRevoked(X509Certificate2 caCert, X509Certificate2 cert)
        {
            string caName = caCert.Subject;
            return RevokedByCa.ContainsKey(caName) && RevokedByCa[caName].Contains(cert.SerialNumber);
        }
    }

    public static class X509Certificate2Extensions
    {
        public static string PfxPath(this X509Certificate2 cert)
        {
            if (cert.Subject.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
                return Path.Combine("Certificates", "OrganizerCA.pfx");
            if (cert.Subject.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
                return Path.Combine("Certificates", "VoterCA.pfx");
            throw new InvalidOperationException("Unknown CA certificate.");
        }
    }
}
