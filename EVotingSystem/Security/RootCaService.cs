using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace EVotingSystem.Security.Ca
{
    public static class RootCaService
    {
        private static X509Certificate2 rootCa;
        private const string CertFolder = "Certificates";
        private const string RootCaFile = "RootCA.pfx";
        private const string RootCaPassword = "RootCAPassword123!"; // can later derive dynamically

        public static X509Certificate2 GetOrCreateRootCa()
        {
            if (!Directory.Exists(CertFolder))
                Directory.CreateDirectory(CertFolder);

            string path = Path.Combine(CertFolder, RootCaFile);

            if (File.Exists(path))
            {
                rootCa = new X509Certificate2(path, RootCaPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                return rootCa;
            }

            using RSA rsa = RSA.Create(4096);

            var request = new CertificateRequest(
                "CN=EVoting Root CA",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(
                    certificateAuthority: true,
                    hasPathLengthConstraint: true,
                    pathLengthConstraint: 1,
                    critical: true));

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                    true));

            var cert = request.CreateSelfSigned(
                DateTimeOffset.Now,
                DateTimeOffset.Now.AddYears(10));

            rootCa = cert; // already has private key

            // Save to disk as PFX
            File.WriteAllBytes(path, rootCa.Export(X509ContentType.Pfx, RootCaPassword));

            return rootCa;
        }
    }
}
