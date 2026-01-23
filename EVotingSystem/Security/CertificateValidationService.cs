using System;
using System.Security.Cryptography.X509Certificates;
using EVotingSystem.Security.Ca;
using EVotingSystem.Security.Crl;

namespace EVotingSystem.Security
{
    public static class CertificateValidationService
    {
        /// <summary>
        /// Validates a user certificate's public part only.
        /// This is used when the user selects a .cer file in Step 1.
        /// Checks expiry, chain, and issuer, but does NOT require private key or PFX password.
        /// </summary>
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

            // Add intermediate CAs
            chain.ChainPolicy.ExtraStore.Add(OrganizerCaService.GetOrCreateCa());
            chain.ChainPolicy.ExtraStore.Add(VoterCaService.GetOrCreateCa());

            if (!chain.Build(cert))
                throw new InvalidOperationException("Certificate chain validation failed.");

            if (CrlService.IsRevoked(cert))
                throw new InvalidOperationException("Sertifikat je opozvan (CRL).");

            // Ensure certificate was issued by a valid EVoting CA
            string issuer = cert.Issuer;
            if (!issuer.Contains("EVoting Organizer CA") && !issuer.Contains("EVoting Voter CA"))
                throw new InvalidOperationException("Certificate issued by an invalid CA.");
        }

        /// <summary>
        /// Validates a full user certificate including private key.
        /// This is used after the user enters their password and we can load the PFX.
        /// Checks expiry, chain, issuer, and ensures private key is accessible with PFX password.
        /// </summary>
        public static void ValidateFullUserCertificate(X509Certificate2 cert)
        {
            if (cert == null)
                throw new InvalidOperationException("No certificate provided.");

            // Step 1 checks (expiry, chain, issuer)
            ValidatePublicUserCertificate(cert);

            // Step 2: ensure private key exists
            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("Certificate does not contain a private key.");
        }
    }
}
