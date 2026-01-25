using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EVotingSystem.Models;
using EVotingSystem.Persistence;

namespace EVotingSystem.Security
{
    public static class VoteService
    {
        public static void CastVote(
            Voting voting,
            Voter voter,
            string selectedOption,
            X509Certificate2 voterCertificate,
            X509Certificate2 organizerCertificate)
        {
            if (voting.Status != VotingStatus.Active)
                throw new InvalidOperationException("Glasanje nije aktivno.");

            // Serialize vote choice

            byte[] voteBytes = Encoding.UTF8.GetBytes(selectedOption);

            // Generate AES key

            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            // Encrypt vote (AES)

            byte[] encryptedVote;
            using (var encryptor = aes.CreateEncryptor())
            {
                encryptedVote = encryptor.TransformFinalBlock(
                    voteBytes, 0, voteBytes.Length);
            }

            // Encrypt AES key (Organizer public key)

            using RSA organizerRsa = organizerCertificate.GetRSAPublicKey()!;
            byte[] encryptedAesKey = organizerRsa.Encrypt(
                aes.Key, RSAEncryptionPadding.OaepSHA256);

            // Sign encrypted vote (Voter private key)

            using RSA voterRsa = voterCertificate.GetRSAPrivateKey()!;
            byte[] signature = voterRsa.SignData(
                encryptedVote,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            // Create vote object

            var encryptedVoteObj = new EncryptedVote
            {
                VotingId = voting.Id,
                VoterId = voter.Id,
                EncryptedChoice = encryptedVote,
                Iv = aes.IV,
                EncryptedAesKey = encryptedAesKey,
                Signature = signature,
                Timestamp = DateTime.UtcNow
            };

            // Create metadata + HMAC

            byte[] metadataBytes = Encoding.UTF8.GetBytes(
                $"{encryptedVoteObj.VoteId}|{encryptedVoteObj.VotingId}|{encryptedVoteObj.VoterId}|{encryptedVoteObj.Timestamp:o}");

            using var hmac = new HMACSHA256(HmacKey.MetadataHmacKey);
            byte[] hmacValue = hmac.ComputeHash(metadataBytes);

            var metadata = new VoteMetadata
            {
                VoteId = encryptedVoteObj.VoteId,
                VotingId = encryptedVoteObj.VotingId,
                VoterId = encryptedVoteObj.VoterId,
                Timestamp = encryptedVoteObj.Timestamp,
                Hmac = hmacValue
            };

            // Export vote

            VoteRepository.AddVote(encryptedVoteObj, metadata);
        }
    }
}
