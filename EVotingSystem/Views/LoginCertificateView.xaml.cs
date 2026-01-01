using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using EVotingSystem.Security.Ca;
using EVotingSystem.Security;

namespace EVotingSystem.Views
{
    public partial class LoginCertificateView : UserControl
    {
        public string? SelectedCertificatePath { get; private set; }
        public X509Certificate2? SelectedCertificate { get; private set; }

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
                // 1️⃣ Load the certificate from the selected file
                SelectedCertificatePath = dialog.FileName;
                SelectedCertificate = new X509Certificate2(File.ReadAllBytes(SelectedCertificatePath));

                // 2️⃣ Validate Step 1: expiry, issuer, chain (no private key required)
                CertificateValidationService.ValidatePublicUserCertificate(SelectedCertificate);

                // 3️⃣ Update UI
                CertificateStatusText.Text = "Sertifikat validan ✔";
                CertificateStatusText.Foreground = Brushes.Green;
                ContinueButton.IsEnabled = true;
            }
            catch
            {
                SelectedCertificatePath = null;
                SelectedCertificate = null;
                CertificateStatusText.Text = "Sertifikat nije validan ✖";
                CertificateStatusText.Foreground = Brushes.Red;
                ContinueButton.IsEnabled = false;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCertificate == null || SelectedCertificatePath == null)
                return;

            // Navigate to credentials step, passing the loaded certificate
            ((MainWindow)Application.Current.MainWindow).MainContent.Content =
                new LoginCredentialsView(SelectedCertificate);
        }

        private void Register_Click(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).MainContent.Content =
                new RegisterView();
        }
    }
}
