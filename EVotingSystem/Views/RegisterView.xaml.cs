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
using Path = System.IO.Path;

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
                .MainContent.Content = new LoginCertificateView();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            User newUser = GetUserFromForm();
            if (newUser == null)
                return;

            //Generate salt for key protection
            byte[] salt = KeyProtectionService.GenerateSalt();
            newUser.KeySalt = salt;

            //Derive PFX password from user's password
            string pfxPassword = KeyProtectionService.DerivePfxPassword(
                newUser.Password,
                salt);

            //Issue certificate
            var cert = CertificateIssuer.IssueUserCertificate(newUser);

            //Create certificate folder if not exists
            string certDir = "Certificates";
            Directory.CreateDirectory(certDir);

            //Export PFX (private key + public key, password protected)
            string pfxPath = Path.Combine(certDir, $"{newUser.Id}.pfx");
            byte[] pfxData = cert.Export(X509ContentType.Pfx, pfxPassword);
            File.WriteAllBytes(pfxPath, pfxData);

            //Export CER (public key only)
            string cerPath = Path.Combine(certDir, $"{newUser.Id}.cer");
            byte[] cerData = cert.Export(X509ContentType.Cert); // public key only
            File.WriteAllBytes(cerPath, cerData);

            //Store paths in user
            newUser.CertificatePath = pfxPath;   // private cert
            newUser.PublicCertPath = cerPath;    // public cert for step 1 of login
            UserRepository.AddUser(newUser);
            MessageBox.Show($"User registered successfully!");
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
