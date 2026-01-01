using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;
using EVotingSystem.Security.Ca;

namespace EVotingSystem.Views
{
    public partial class VoterMainView : UserControl
    {
        public VoterMainView()
        {
            InitializeComponent();
            LoadActiveVotings();
        }

        private void LoadActiveVotings()
        {
            var votings = VotingRepository.GetAllVotings();
            VotingsList.ItemsSource = votings;

        }


        private void VotingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VotingsList.SelectedItem is not Voting selectedVoting)
                return;

            if (selectedVoting.Status != VotingStatus.Active)
            {
                MessageBox.Show(
                    selectedVoting.Status == VotingStatus.NotStarted
                        ? "Glasanje još nije počelo."
                        : "Glasanje je završeno."
                );

                VotingsList.SelectedItem = null;
                return;
            }


            var voter = SessionContext.CurrentUser as Voter;
            var voterCert = SessionContext.UserCertificate;

            if (voter == null || voterCert == null)
            {
                MessageBox.Show("Session error. Please log in again.");
                return;
            }

            // Organizer certificate (used to encrypt symmetric key)
            var organizerCert = OrganizerCaService.GetOrCreateCa();

            ((MainWindow)Application.Current.MainWindow).MainContent.Content =
                new VoteMenuView(
                    selectedVoting,
                    voter,
                    voterCert,
                    organizerCert);
        }
    }
}
