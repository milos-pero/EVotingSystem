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
    /// <summary>
    /// Interaction logic for RegisterView.xaml
    /// </summary>
    public partial class RegisterView : UserControl
    {
        private UserType selectedUserType = UserType.None;

        public RegisterView()
        {
            InitializeComponent();
            RegisterButton.IsEnabled = false; // disable initially
        }

        private void Organizer_Click(object sender, RoutedEventArgs e)
        {
            selectedUserType = UserType.Organizer;
            FormContent.Content = new OrganizerRegisterForm();
            RegisterButton.IsEnabled = true; // enable now
        }

        private void Voter_Click(object sender, RoutedEventArgs e)
        {
            selectedUserType = UserType.Voter;
            FormContent.Content = new VoterRegisterForm();
            RegisterButton.IsEnabled = true; // enable now
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new LoginView();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Here you can later handle registration using selectedUserType
            MessageBox.Show($"Registering as {selectedUserType}");
        }
    }


}
