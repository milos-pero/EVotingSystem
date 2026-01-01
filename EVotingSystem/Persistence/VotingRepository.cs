using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EVotingSystem.Models;

namespace EVotingSystem.Persistence
{
    public static class VotingRepository
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "votings.json");

        static VotingRepository()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            if (!File.Exists(FilePath))
                File.WriteAllText(FilePath, "[]");
        }

        public static void AddVoting(Voting voting)
        {
            var votings = GetAllVotings();
            votings.Add(voting);
            SaveAll(votings);
        }

        public static List<Voting> GetAllVotings()
        {
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Voting>>(json)!;
        }

        public static List<Voting> GetActiveVotings()
        {
            return GetAllVotings()
                .Where(v => v.Status == VotingStatus.Active)
                .ToList();
        }

        private static void SaveAll(List<Voting> votings)
        {
            var json = JsonSerializer.Serialize(votings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }
    }
}
