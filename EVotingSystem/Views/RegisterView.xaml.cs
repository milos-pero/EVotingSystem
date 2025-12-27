using EVotingSystem.Models;
using EVotingSystem.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            if (newUser == null)
                return;

            // 1️⃣ Generate salt for key protection
            byte[] salt = KeyProtectionService.GenerateSalt();
            newUser.KeySalt = salt;

            // 2️⃣ Derive PFX password from user's password
            string pfxPassword = KeyProtectionService.DerivePfxPassword(
                newUser.Password,
                salt);

            // 3️⃣ Issue certificate
            var cert = CertificateIssuer.IssueUserCertificate(newUser);

            // 4️⃣ Save certificate securely
            string certDir = "Certificates";
            Directory.CreateDirectory(certDir);

            string certPath = System.IO.Path.Combine(
                certDir,
                $"{newUser.Id}.pfx");

            byte[] pfxData = cert.Export(
                X509ContentType.Pfx,
                pfxPassword);

            File.WriteAllBytes(certPath, pfxData);

            newUser.CertificatePath = certPath;

            MessageBox.Show("User registered and certificate securely stored!");
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
