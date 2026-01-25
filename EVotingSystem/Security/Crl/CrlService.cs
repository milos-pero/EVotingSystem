using EVotingSystem.Security.Ca;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace EVotingSystem.Security.Crl
{
    public static class CrlService
    {
        private const string CrlFolder = "Certificates/CRL";
        private static readonly Dictionary<string, HashSet<BigInteger>> RevokedByCa = new();

        private static string GetCrlPath(Org.BouncyCastle.X509.X509Certificate caCert)
        {
            Directory.CreateDirectory(CrlFolder);

            string safeName = caCert.SubjectDN.ToString()
                .Replace(" ", "_")
                .Replace(",", "_")
                .Replace("/", "_");

            return Path.Combine(CrlFolder, $"{safeName}.crl");
        }

        public static void RevokeCertificate(X509Certificate2 certToRevoke)
        {
            // Load the CA PFX
            string pfxPath;
            string password;

            if (certToRevoke.Issuer.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
            {
                pfxPath = "Certificates/OrganizerCA.pfx";
                password = "organizer";
            }
            else if (certToRevoke.Issuer.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
            {
                pfxPath = "Certificates/VoterCA.pfx";
                password = "voter";
            }
            else
            {
                throw new InvalidOperationException("Cannot revoke certificate: unknown CA issuer.");
            }

            // Load CA certificate and private key
            (Org.BouncyCastle.X509.X509Certificate caCert, AsymmetricKeyParameter caPrivateKey) = LoadCaFromPfx(pfxPath, password);

            string caName = caCert.SubjectDN.ToString();
            if (!RevokedByCa.ContainsKey(caName))
                RevokedByCa[caName] = new HashSet<BigInteger>();

            // Add revoked certificate serial
            RevokedByCa[caName].Add(new BigInteger(certToRevoke.SerialNumber, 16));

            // Generate updated CRL
            GenerateCrl(caCert, caPrivateKey, RevokedByCa[caName]);
        }

        // Generate a CRL for the CA
        private static void GenerateCrl(Org.BouncyCastle.X509.X509Certificate caCert, AsymmetricKeyParameter caPrivateKey, HashSet<BigInteger> revokedSerials)
        {
            string crlPath = GetCrlPath(caCert);

            var crlGen = new X509V2CrlGenerator();
            crlGen.SetIssuerDN(caCert.SubjectDN);
            crlGen.SetThisUpdate(DateTime.UtcNow);
            crlGen.SetNextUpdate(DateTime.UtcNow.AddYears(1));

            foreach (var serial in revokedSerials)
            {
                crlGen.AddCrlEntry(serial, DateTime.UtcNow, CrlReason.PrivilegeWithdrawn);
            }

            var sigFactory = new Asn1SignatureFactory("SHA256WITHRSA", caPrivateKey);
            var crl = crlGen.Generate(sigFactory);

            File.WriteAllBytes(crlPath, crl.GetEncoded());
        }

        // Check if a certificate is revoked
        public static bool IsRevoked(X509Certificate2 cert)
        {
            // Load CA cert
            X509Certificate2 caCert2;

            if (cert.Issuer.Contains("EVoting Organizer CA", StringComparison.OrdinalIgnoreCase))
                caCert2 = OrganizerCaService.GetOrCreateCa();
            else if (cert.Issuer.Contains("EVoting Voter CA", StringComparison.OrdinalIgnoreCase))
                caCert2 = VoterCaService.GetOrCreateCa();
            else
                throw new InvalidOperationException("Unknown CA issuer");

            // Convert CA to BouncyCastle
            Org.BouncyCastle.X509.X509Certificate caCert = DotNetUtilities.FromX509Certificate(caCert2);
            string caName = caCert.SubjectDN.ToString();

            if (!RevokedByCa.ContainsKey(caName))
                LoadCrl(caCert);

            return RevokedByCa.ContainsKey(caName) &&
                   RevokedByCa[caName].Contains(new BigInteger(cert.SerialNumber, 16));
        }

        // Load into memory
        private static void LoadCrl(Org.BouncyCastle.X509.X509Certificate caCert)
        {
            string crlPath = GetCrlPath(caCert);
            string caName = caCert.SubjectDN.ToString();

            RevokedByCa[caName] = new HashSet<BigInteger>();

            if (!File.Exists(crlPath))
                return;

            var crlParser = new X509CrlParser();
            var crl = crlParser.ReadCrl(File.ReadAllBytes(crlPath));

            var revoked = crl.GetRevokedCertificates();
            if (revoked != null)
            {
                foreach (X509CrlEntry entry in revoked)
                {
                    RevokedByCa[caName].Add(entry.SerialNumber);
                }
            }
        }

        // Load CA certificate and private key
        private static (Org.BouncyCastle.X509.X509Certificate, AsymmetricKeyParameter) LoadCaFromPfx(string pfxPath, string password)
        {
            using var fs = File.OpenRead(pfxPath);
            var store = new Pkcs12Store(fs, password.ToCharArray());

            string alias = store.Aliases.Cast<string>().First(a => store.IsKeyEntry(a));
            var key = store.GetKey(alias).Key;
            var cert = store.GetCertificate(alias).Certificate;

            return (cert, key);
        }
    }
}
