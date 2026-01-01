using System.Windows;
using System.Windows.Controls;

namespace EVotingSystem.Views
{
    public partial class OrganizerMainView : UserControl
    {
        public OrganizerMainView()
        {
            InitializeComponent();
        }

        private void CreateVoting_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new CreateVotingView();
        }

        private void ViewVotings_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new OrganizerVotingsView();
        }
    }
}
