using System;
using System.Collections.Generic;

namespace EVotingSystem.Models
{
    public class Voting
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> Options { get; set; } = new();

        public VotingStatus Status
        {
            get
            {
                var now = DateTime.Now;

                if (now < StartTime)
                    return VotingStatus.NotStarted;

                if (now > EndTime)
                    return VotingStatus.Finished;

                return VotingStatus.Active;
            }
        }


    }

    public enum VotingStatus
    {
        NotStarted,
        Active,
        Finished
    }

}
