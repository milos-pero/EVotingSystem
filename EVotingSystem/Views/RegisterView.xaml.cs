using EVotingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EVotingSystem.Views
{
    public partial class RegisterView : UserControl
    {
        private UserType selectedUserType = UserType.None;

        public RegisterView()
        {
            InitializeComponent();
            RegisterButton.IsEnabled = false; // Disable until role selected
        }

        private void Organizer_Click(object sender, RoutedEventArgs e)
        {
            selectedUserType = UserType.Organizer;
            FormContent.Content = new OrganizerRegisterForm();
            RegisterButton.IsEnabled = true;
        }

        private void Voter_Click(object sender, RoutedEventArgs e)
        {
            selectedUserType = UserType.Voter;
            FormContent.Content = new VoterRegisterForm();
            RegisterButton.IsEnabled = true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new LoginView();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            User newUser = GetUserFromForm();

            // For now, just show a message to test
            if (newUser is Organizer organizer)
            {
                MessageBox.Show($"Organizer registered: {organizer.OrganizationName}, ID: {organizer.OrganizationId}");
            }
            else if (newUser is Voter voter)
            {
                MessageBox.Show($"Voter registered: {voter.FirstName} {voter.LastName}, Username: {voter.Username}");
            }

            // Later: handle certificate generation & storage
        }

        private User GetUserFromForm()
        {
            if (selectedUserType == UserType.Organizer)
            {
                var form = FormContent.Content as OrganizerRegisterForm;
                if (form != null)
                {
                    return new Organizer
                    {
                        OrganizationName = form.OrganizationNameTextBox.Text,
                        OrganizationId = form.OrganizationIdTextBox.Text,
                        Password = form.PasswordBox.Password
                    };
                }
            }
            else if (selectedUserType == UserType.Voter)
            {
                var form = FormContent.Content as VoterRegisterForm;
                if (form != null)
                {
                    return new Voter
                    {
                        FirstName = form.FirstNameTextBox.Text,
                        LastName = form.LastNameTextBox.Text,
                        Username = form.UsernameTextBox.Text,
                        Password = form.PasswordBox.Password
                    };
                }
            }

            return null; // should never happen if button is disabled until selection
        }
    }
}
