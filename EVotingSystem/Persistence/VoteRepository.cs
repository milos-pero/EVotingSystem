using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EVotingSystem.Models;

namespace EVotingSystem.Persistence
{
    public static class VoteRepository
    {
        private static readonly string DataDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "votes");

        private static readonly string VotesFile =
            Path.Combine(DataDir, "encrypted_votes.json");

        private static readonly string MetadataFile =
            Path.Combine(DataDir, "vote_metadata.json");

        static VoteRepository()
        {
            Directory.CreateDirectory(DataDir);

            if (!File.Exists(VotesFile))
                File.WriteAllText(VotesFile, "[]");

            if (!File.Exists(MetadataFile))
                File.WriteAllText(MetadataFile, "[]");
        }

        // ---------------------------
        // PUBLIC API
        // ---------------------------

        public static void AddVote(EncryptedVote vote, VoteMetadata metadata)
        {
            var votes = LoadAllVotes();
            var metadataList = LoadAllMetadata();

            votes.Add(vote);
            metadataList.Add(metadata);

            SaveAllVotes(votes);
            SaveAllMetadata(metadataList);
        }

        public static List<VoteMetadata> GetMetadataForVoter(Guid voterId)
        {
            return LoadAllMetadata()
                .Where(m => m.VoterId == voterId)
                .ToList();
        }

        public static List<EncryptedVote> GetVotesForVoting(Guid votingId)
        {
            return LoadAllVotes()
                .Where(v => v.VotingId == votingId)
                .ToList();
        }
        public static bool HasVoterAlreadyVoted(Guid voterId, Guid votingId)
        {
            var votes = LoadAllVotes(); // read all encrypted votes
            return votes.Any(v => v.VoterId == voterId && v.VotingId == votingId);
        }


        // ---------------------------
        // INTERNAL STORAGE
        // ---------------------------

        private static List<EncryptedVote> LoadAllVotes()
        {
            string json = File.ReadAllText(VotesFile);
            return JsonSerializer.Deserialize<List<EncryptedVote>>(json)!;
        }

        private static void SaveAllVotes(List<EncryptedVote> votes)
        {
            string json = JsonSerializer.Serialize(votes, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(VotesFile, json);
        }

        private static List<VoteMetadata> LoadAllMetadata()
        {
            string json = File.ReadAllText(MetadataFile);
            return JsonSerializer.Deserialize<List<VoteMetadata>>(json)!;
        }

        private static void SaveAllMetadata(List<VoteMetadata> metadata)
        {
            string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(MetadataFile, json);
        }
    }
}
