using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;
using System.Windows.Threading;


namespace EVotingSystem.Views
{
    public partial class VoteMenuView : UserControl
    {
        private readonly Voting voting;
        private readonly Voter voter;
        private readonly X509Certificate2 voterCert;
        private readonly X509Certificate2 organizerCert;
        private DispatcherTimer _statusTimer;


        public VoteMenuView(
            Voting voting,
            Voter voter,
            X509Certificate2 voterCert,
            X509Certificate2 organizerCert)
        {
            InitializeComponent();

            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _statusTimer.Tick += (s, e) => UpdateVoteButtonState();
            _statusTimer.Start();

            this.voting = voting;
            this.voter = voter;
            this.voterCert = voterCert;
            this.organizerCert = organizerCert;

            TitleText.Text = voting.Title;
            DescriptionText.Text = voting.Description;
            PeriodText.Text = $"{voting.StartTime:G} - {voting.EndTime:G}";

            BuildOptions();
            UpdateVoteButtonState();

        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new VoterMainView();
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
                MessageBox.Show("Morate izabrati jednu opciju.");
                return;
            }

            string selectedOption = selectedRadio.Content.ToString()!;

            // -----------------------------
            // Prevent double voting
            // -----------------------------
            bool alreadyVoted = VoteRepository.HasVoterAlreadyVoted(voter.Id, voting.Id);
            if (alreadyVoted)
            {
                MessageBox.Show("Vec ste glasali.");
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

                MessageBox.Show("Uspesno ste glasali.");

                ((MainWindow)Application.Current.MainWindow)
                    .MainContent.Content = new VoterMainView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Glasanje neuspesno: {ex.Message}");
            }
        }
        private void UpdateVoteButtonState()
        {
            switch (voting.Status)
            {
                case VotingStatus.NotStarted:
                    VoteButton.IsEnabled = false;
                    VoteButton.Content = "Glasanje još nije počelo";
                    break;

                case VotingStatus.Active:
                    VoteButton.IsEnabled = true;
                    VoteButton.Content = "Glasaj";
                    break;

                case VotingStatus.Finished:
                    VoteButton.IsEnabled = false;
                    VoteButton.Content = "Glasanje je završeno";
                    break;
            }
        }


    }
}
