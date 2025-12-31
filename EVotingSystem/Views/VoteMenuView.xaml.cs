using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;

namespace EVotingSystem.Views
{
    public partial class VoteMenuView : UserControl
    {
        private readonly Voting voting;
        private readonly Voter voter;
        private readonly X509Certificate2 voterCert;
        private readonly X509Certificate2 organizerCert;

        public VoteMenuView(
            Voting voting,
            Voter voter,
            X509Certificate2 voterCert,
            X509Certificate2 organizerCert)
        {
            InitializeComponent();

            this.voting = voting;
            this.voter = voter;
            this.voterCert = voterCert;
            this.organizerCert = organizerCert;

            TitleText.Text = voting.Title;
            DescriptionText.Text = voting.Description;
            PeriodText.Text = $"{voting.StartTime:G} - {voting.EndTime:G}";

            BuildOptions();
        }

        private void BuildOptions()
        {
            OptionsPanel.Children.Clear();

            foreach (string option in voting.Options)
            {
                var radio = new RadioButton
                {
                    Content = option,
                    GroupName = "VotingOptions",
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                OptionsPanel.Children.Add(radio);
            }
        }

        private void Vote_Click(object sender, RoutedEventArgs e)
        {
            // Find selected radio button
            var selectedRadio = OptionsPanel.Children
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true);

            if (selectedRadio == null)
            {
                MessageBox.Show("Please select an option.");
                return;
            }

            string selectedOption = selectedRadio.Content.ToString()!;

            // -----------------------------
            // Prevent double voting
            // -----------------------------
            bool alreadyVoted = VoteRepository.HasVoterAlreadyVoted(voter.Id, voting.Id);
            if (alreadyVoted)
            {
                MessageBox.Show("You have already voted in this election.");
                return;
            }

            try
            {
                VoteService.CastVote(
                    voting,
                    voter,
                    selectedOption,
                    voterCert,
                    organizerCert);

                MessageBox.Show("Your vote has been successfully recorded.");

                ((MainWindow)Application.Current.MainWindow)
                    .MainContent.Content = new VoterMainView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Voting failed: {ex.Message}");
            }
        }

    }
}
