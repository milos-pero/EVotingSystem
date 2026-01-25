using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EVotingSystem.Models;

namespace EVotingSystem.Security
{
    public static class SignatureValidationService
    {
        public static bool VerifyVoteSignature(
            EncryptedVote vote,
            X509Certificate2 voterCertificate)
        {
            if (vote == null || voterCertificate == null)
                return false;

            using RSA rsa = voterCertificate.GetRSAPublicKey()!;
            if (rsa == null)
                return false;

            return rsa.VerifyData(
                vote.EncryptedChoice,
                vote.Signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}
