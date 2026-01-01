using EVotingSystem.Models;
using EVotingSystem.Persistence;
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

            // --- VALIDATE UNIQUENESS ---

            // Check for duplicate names across all users
            bool nameExists = UserRepository.GetAllUsers().Any(u =>
            {
                if (newUser is Voter nv && u is Voter v)
                    return nv.FirstName.Equals(v.FirstName, StringComparison.OrdinalIgnoreCase)
                        && nv.LastName.Equals(v.LastName, StringComparison.OrdinalIgnoreCase);

                if (newUser is Organizer no && u is Organizer o)
                    return no.OrganizationName.Equals(o.OrganizationName, StringComparison.OrdinalIgnoreCase);

                return false;
            });

            if (nameExists)
            {
                MessageBox.Show("A user with the same name or organization name already exists.");
                return;
            }

            // For organizers: also check duplicate OrganizationId
            if (newUser is Organizer newOrg)
            {
                bool idExists = UserRepository.GetAllUsers().OfType<Organizer>()
                    .Any(o => o.OrganizationId.Equals(newOrg.OrganizationId, StringComparison.OrdinalIgnoreCase));

                if (idExists)
                {
                    MessageBox.Show("An organizer with the same identification number already exists.");
                    return;
                }
            }

            // --- CONTINUE REGISTRATION ---
            byte[] salt = KeyProtectionService.GenerateSalt();
            newUser.KeySalt = salt;

            string pfxPassword = KeyProtectionService.DerivePfxPassword(newUser.Password, salt);

            var cert = CertificateIssuer.IssueUserCertificate(newUser);

            string certDir = "Certificates";
            Directory.CreateDirectory(certDir);

            string pfxPath = Path.Combine(certDir, $"{newUser.Id}.pfx");
            File.WriteAllBytes(pfxPath, cert.Export(X509ContentType.Pfx, pfxPassword));

            string cerPath = Path.Combine(certDir, $"{newUser.Id}.cer");
            File.WriteAllBytes(cerPath, cert.Export(X509ContentType.Cert));

            newUser.CertificatePath = pfxPath;
            newUser.PublicCertPath = cerPath;

            UserRepository.AddUser(newUser);
            MessageBox.Show("User registered successfully!");
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
