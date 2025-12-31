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
            var activeVotings = VotingRepository.GetActiveVotings();

            if (activeVotings.Any())
            {
                VotingsList.ItemsSource = activeVotings;
            }
            else
            {
                VotingsList.Items.Clear();
                VotingsList.Items.Add("Trenutno nema aktivnih glasanja.");
            }
        }

        private void VotingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VotingsList.SelectedItem is not Voting selectedVoting)
                return;

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
