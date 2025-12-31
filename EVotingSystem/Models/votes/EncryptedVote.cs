namespace EVotingSystem.Models
{
    public class EncryptedVote
    {
        public Guid VoteId { get; set; } = Guid.NewGuid();
        public Guid VotingId { get; set; }
        public Guid VoterId { get; set; }

        public byte[] EncryptedChoice { get; set; }
        public byte[] Iv { get; set; }
        public byte[] EncryptedAesKey { get; set; }
        public byte[] Signature { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
