using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EVotingSystem.Security.Ca
{
    public static class VoterCaService
    {
        private static X509Certificate2 voterCa;

        private const string CertFolder = "Certificates";
        private const string VoterCaFile = "VoterCA.pfx";
        private const string VoterCaPassword = "VoterCAPassword!"; // can later derive dynamically

        public static X509Certificate2 GetOrCreateCa()
        {
            if (!Directory.Exists(CertFolder))
                Directory.CreateDirectory(CertFolder);

            string path = Path.Combine(CertFolder, VoterCaFile);

            if (File.Exists(path))
            {
                voterCa = new X509Certificate2(path, VoterCaPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                return voterCa;
            }

            var rootCa = RootCaService.GetOrCreateRootCa();

            using RSA rsa = RSA.Create(4096);

            var request = new CertificateRequest(
                "CN=EVoting Voter CA",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(
                    certificateAuthority: true,
                    hasPathLengthConstraint: false,
                    pathLengthConstraint: 0,
                    critical: true));

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                    true));

            var cert = request.Create(
                rootCa,
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(5),
                Guid.NewGuid().ToByteArray());

            voterCa = cert.CopyWithPrivateKey(rsa);

            // Save to disk as PFX
            File.WriteAllBytes(path, voterCa.Export(X509ContentType.Pfx, VoterCaPassword));

            return voterCa;
        }
    }
}
