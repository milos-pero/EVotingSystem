using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using EVotingSystem.Security;
using EVotingSystem.Models;
using EVotingSystem.Security.Ca;

namespace EVotingSystem.Views
{
    public partial class LoginCertificateView : UserControl
    {
        public string? SelectedCertificatePath { get; private set; }
        public X509Certificate2? SelectedCertificate { get; private set; }
        public UserType? CertificateUserType { get; private set; }

        public LoginCertificateView()
        {
            InitializeComponent();
            ContinueButton.IsEnabled = false;
        }

        private void SelectCertificate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Certificate Files (*.cer)|*.cer",
                Title = "Select your public digital certificate"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                SelectedCertificatePath = dialog.FileName;
                SelectedCertificate = new X509Certificate2(
                    File.ReadAllBytes(SelectedCertificatePath));

                // Standard certificate validation (expiry, chain, CRL)
                CertificateValidationService
                    .ValidatePublicUserCertificate(SelectedCertificate);

                // Determine user type from issuer
                CertificateUserType = GetUserTypeFromIssuer(SelectedCertificate);

                // success
                CertificateStatusText.Text =
                    $"Sertifikat je validan ✔";
                CertificateStatusText.Foreground = Brushes.Green;
                ContinueButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                SelectedCertificate = null;
                SelectedCertificatePath = null;
                CertificateUserType = null;

                CertificateStatusText.Text = $"Sertifikat nije validan ✖\n{ex.Message}";
                CertificateStatusText.Foreground = Brushes.Red;
                ContinueButton.IsEnabled = false;
            }
        }

        private static UserType GetUserTypeFromIssuer(X509Certificate2 cert)
        {
            var organizerCa = OrganizerCaService.GetOrCreateCa();
            var voterCa = VoterCaService.GetOrCreateCa();

            if (cert.IssuerName.RawData.SequenceEqual(organizerCa.SubjectName.RawData))
                return UserType.Organizer;

            if (cert.IssuerName.RawData.SequenceEqual(voterCa.SubjectName.RawData))
                return UserType.Voter;

            throw new InvalidOperationException(
                "Sertifikat nije izdat od validnog CA.");
        }


        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCertificate == null || CertificateUserType == null)
                return;

            ((MainWindow)Application.Current.MainWindow).MainContent.Content =
                new LoginCredentialsView( SelectedCertificate);
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainContent.Content =
                new RegisterView();
        }
    }
}
