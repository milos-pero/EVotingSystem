using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;

namespace EVotingSystem.Views
{
    public partial class LoginCredentialsView : UserControl
    {
        private readonly X509Certificate2 publicCertificate;

        public LoginCredentialsView(X509Certificate2 certificate)
        {
            InitializeComponent();
            publicCertificate = certificate;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            try
            {
                // Find the user by username (replace this with your actual user storage)
                User? user = UserRepository.FindUserForLogin(username);

                if (user == null)
                {
                    MessageBox.Show("User not found.");
                    return;
                }

                // Verify password hash (assuming your User object stores password in plain for now)
                if (user.Password != password)
                {
                    MessageBox.Show("Incorrect password.");
                    return;
                }

                // Derive PFX password from user password + salt
                string pfxPassword = KeyProtectionService.DerivePfxPassword(password, user.KeySalt!);

                // Load the user's PFX file
                if (!File.Exists(user.CertificatePath!))
                {
                    MessageBox.Show("Certificate file not found.");
                    return;
                }

                using X509Certificate2 fullCert = new X509Certificate2(
                    File.ReadAllBytes(user.CertificatePath!),
                    pfxPassword,
                    X509KeyStorageFlags.Exportable);

                // Validate full certificate (private key, issuer, chain)
                CertificateValidationService.ValidateFullUserCertificate(fullCert);

                // Ensure the public certificate from Step 1 matches the full certificate
                if (!fullCert.Thumbprint.Equals(publicCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The selected certificate does not belong to this user.");
                    return;
                }

                // Login successful
                if (user is Organizer)
                {
                    ((MainWindow)Application.Current.MainWindow)
                        .MainContent.Content = new OrganizerMainView();
                }
                else if (user is Voter)
                {
                    ((MainWindow)Application.Current.MainWindow)
                        .MainContent.Content = new VoterMainView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}");
            }
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainContent.Content = new RegisterView();
        }
    }
}
