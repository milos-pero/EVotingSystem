using EVotingSystem.Models;
using EVotingSystem.Security;
using System.Windows;
using System.Windows.Controls;

namespace EVotingSystem.Views
{
    public partial class OrganizerMainView : UserControl
    {

        public OrganizerMainView()
        {
            InitializeComponent();

            if (SessionContext.CurrentUser is not Organizer)
            {
                ((MainWindow)Application.Current.MainWindow)
                    .MainContent.Content = new LoginCertificateView();
                return;
            }
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
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Clear session data
            SessionContext.CurrentUser = null;
            SessionContext.UserCertificate = null;

            // Navigate back to login
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new LoginCertificateView();
        }
    }
}
