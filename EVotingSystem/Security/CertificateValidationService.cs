using System;
using System.Security.Cryptography.X509Certificates;
using EVotingSystem.Security.Ca;
using EVotingSystem.Security.Crl;

namespace EVotingSystem.Security
{
    public static class CertificateValidationService
    {

        // Check expiry, chain, and issuer
        public static void ValidatePublicUserCertificate(X509Certificate2 cert)
        {
            if (cert == null)
                throw new InvalidOperationException("No certificate provided.");

            // Check expiration
            if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
                throw new InvalidOperationException("Certificate is expired or not yet valid.");

            using var chain = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.NoFlag,
                    TrustMode = X509ChainTrustMode.CustomRootTrust
                }
            };

            // Add trusted Root CA
            chain.ChainPolicy.CustomTrustStore.Add(RootCaService.GetOrCreateRootCa());

            // Add CAs
            chain.ChainPolicy.ExtraStore.Add(OrganizerCaService.GetOrCreateCa());
            chain.ChainPolicy.ExtraStore.Add(VoterCaService.GetOrCreateCa());

            if (!chain.Build(cert))
                throw new InvalidOperationException("Certificate chain validation failed.");


            if (CrlService.IsRevoked(cert))
                throw new InvalidOperationException("Sertifikat je opozvan (CRL).");

            // Ensure certificate was issued by a valid CA
            var organizerCa = OrganizerCaService.GetOrCreateCa();
            var voterCa = VoterCaService.GetOrCreateCa();

            bool issuedByOrganizerCa =
                cert.Issuer == organizerCa.Subject;

            bool issuedByVoterCa =
                cert.Issuer == voterCa.Subject;

            if (!issuedByOrganizerCa && !issuedByVoterCa)
                throw new InvalidOperationException(
                    "Certificate was not issued by a trusted EVoting CA.");
        }

        // Check expiry, chain, issuer, and get private key with PFX password
        public static void ValidateFullUserCertificate(X509Certificate2 cert)
        {
            if (cert == null)
                throw new InvalidOperationException("No certificate provided.");

            // check (expiry, chain, issuer)
            ValidatePublicUserCertificate(cert);

            // ensure private key exists
            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("Certificate does not contain a private key.");
        }
    }
}
