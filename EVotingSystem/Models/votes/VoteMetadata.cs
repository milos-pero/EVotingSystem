namespace EVotingSystem.Models
{
    public class VoteMetadata
    {
        public Guid VoteId { get; set; }
        public Guid VotingId { get; set; }
        public Guid VoterId { get; set; }
        public DateTime Timestamp { get; set; }

        public byte[] Hmac { get; set; }
    }
}
