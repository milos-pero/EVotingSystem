using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;

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

            // Navigate to vote menu
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new VoteMenuView(selectedVoting);

            // Optional: reset selection to prevent double-trigger
            VotingsList.SelectedItem = null;
        }
    }
}
