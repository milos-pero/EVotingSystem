using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;
using EVotingSystem.Persistence;

namespace EVotingSystem.Views
{
    public partial class OrganizerVotingsView : UserControl
    {
        public OrganizerVotingsView()
        {
            InitializeComponent();
            LoadVotings();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new OrganizerMainView();
        }

        private void LoadVotings()
        {
            VotingsList.ItemsSource = VotingRepository.GetAllVotings();
        }

        private void CountVotes_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var voting = button?.DataContext as Voting;

            if (voting == null)
                return;

            ((MainWindow)Application.Current.MainWindow)
                    .MainContent.Content = new VoteCountingView(voting);
        }
    }
}
