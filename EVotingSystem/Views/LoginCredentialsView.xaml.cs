using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EVotingSystem.Models;
using EVotingSystem.Persistence;
using EVotingSystem.Security;
using EVotingSystem.Security.Crl;

namespace EVotingSystem.Views
{
    public partial class LoginCredentialsView : UserControl
    {
        private readonly X509Certificate2 publicCertificate;
        private int loginAttempts = 0;

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
                MessageBox.Show("Unesite korisnicko ime i lozinku.");
                return;
            }

            try
            {
                User? user = UserRepository.FindUserForLogin(username);

                if (user == null)
                {
                    loginFail();
                    MessageBox.Show("Korisnik nije pronadjen.");
                    return;
                }

                if (!PasswordHashService.VerifyPassword(password, user.PasswordHash,user.KeySalt))
                {
                    loginFail();
                    MessageBox.Show("Pogresna lozinka.");
                    return;
                }


                string pfxPassword = KeyProtectionService
                    .DerivePfxPassword(password, user.KeySalt!);

                if (!File.Exists(user.CertificatePath!))
                {
                    loginFail();
                    MessageBox.Show("Sertifikat nije pronadjen.");
                    return;
                }

                X509Certificate2 fullCert = new X509Certificate2(
                    File.ReadAllBytes(user.CertificatePath),
                    pfxPassword,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                // PFX validation
                CertificateValidationService.ValidateFullUserCertificate(fullCert);

                if (!fullCert.Thumbprint.Equals(
                        publicCertificate.Thumbprint,
                        StringComparison.OrdinalIgnoreCase))
                {
                    loginFail();
                    MessageBox.Show("Izabran sertifikat ne odgovara ovom korisniku.");
                    return;
                }

                // reset counter
                loginAttempts = 0;

                SessionContext.CurrentUser = user;
                SessionContext.UserCertificate = fullCert;

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
                loginFail();
                MessageBox.Show($"Prijava neuspesna: {ex.Message}");
            }
        }

        private void loginFail()
        {
            loginAttempts++;

            if (loginAttempts >= 3)
            {
                CrlService.RevokeCertificate(publicCertificate);

                MessageBox.Show(
                    "Sertifikat je opozvan zbog 3 neuspešna pokušaja prijave.",
                    "Pristup odbijen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                loginAttempts = 0;
            }
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new RegisterView();
        }
    }
}
