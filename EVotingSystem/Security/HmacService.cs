using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EVotingSystem.Models;

namespace EVotingSystem.Security
{
    public static class MetadataHmacService
    {
        public static bool VerifyVoteMetadata(VoteMetadata meta)
        {
            if (meta == null)
                return false;

            using var hmac = new HMACSHA256(HmacKey.MetadataHmacKey);

            var data = Encoding.UTF8.GetBytes(
                $"{meta.VoteId}|{meta.VotingId}|{meta.VoterId}|{meta.Timestamp:o}");

            var computed = hmac.ComputeHash(data);
            return computed.SequenceEqual(meta.Hmac);
        }
    }
}
