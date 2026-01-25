using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;
using EVotingSystem.Security.Ca;

namespace EVotingSystem.Views
{
    public partial class VoteCountingView : UserControl
    {
        public List<VoteResultRow> Results { get; set; } = new();
        public string TotalVotesText { get; set; }

        private readonly Voting _voting;

        public VoteCountingView(Voting voting)
        {
            _voting = voting;

            InitializeComponent();
            DataContext = this;

            CountVotes();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new OrganizerVotingsView();
        }
        private void CountVotes()
        {
            var encryptedVotes = VoteRepository.GetVotesForVoting(_voting.Id);
            var metadataList = VoteRepository.GetMetadataForVoting(_voting.Id);

            var optionCounts = _voting.Options.ToDictionary(o => o, _ => 0);
            int validVotes = 0;

            foreach (var vote in encryptedVotes)
            {
                var meta = metadataList.FirstOrDefault(m => m.VoteId == vote.VoteId);
                if (meta == null)
                    continue;

                if (!MetadataHmacService.VerifyVoteMetadata(meta))
                    continue;

                string choice = DecryptVoteChoice(vote);
                if (choice == null)
                    continue;

                if (optionCounts.ContainsKey(choice))
                {
                    optionCounts[choice]++;
                    validVotes++;
                }
            }

            Results = optionCounts
                .Select(kv => new VoteResultRow
                {
                    Option = kv.Key,
                    Count = kv.Value
                })
                .ToList();

            TotalVotesText = $"Ukupan broj važećih glasova: {validVotes}";
        }
        private byte[] DecryptAesKey(
        byte[] encryptedAesKey,
        X509Certificate2 organizerCertificate)
        {
            using RSA rsa = organizerCertificate.GetRSAPrivateKey()!;
            return rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.OaepSHA256);
        }
        private string DecryptVote(
        EncryptedVote vote,
        byte[] aesKey)
        {
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = aesKey;
            aes.IV = vote.Iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(
                vote.EncryptedChoice, 0, vote.EncryptedChoice.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private string? DecryptVoteChoice(EncryptedVote vote)
        {
            // 1. Get organizer private certificate
            var organizerCert = OrganizerCaService.GetOrCreateCa();

            // 2. Get voter public certificate
            var voter = UserRepository.FindById(vote.VoterId);
            if (voter == null || string.IsNullOrEmpty(voter.PublicCertPath))
                return null;

            var voterCert = new X509Certificate2(voter.PublicCertPath);


            // 3. Verify signature FIRST
            if (!SignatureValidationService.VerifyVoteSignature(vote, voterCert))
                return null;

            // 4. Decrypt AES key
            byte[] aesKey = DecryptAesKey(vote.EncryptedAesKey, organizerCert);

            // 5. Decrypt vote
            return DecryptVote(vote, aesKey);
        }

    }
}
