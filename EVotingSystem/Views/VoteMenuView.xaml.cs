using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EVotingSystem.Models;

namespace EVotingSystem.Views
{
    public partial class VoteMenuView : UserControl
    {
        private readonly Voting voting;

        public VoteMenuView(Voting voting)
        {
            InitializeComponent();
            this.voting = voting;
            loadVoting();
        }

        private void loadVoting()
        {
            TitleText.Text = voting.Title;
            DescriptionText.Text = voting.Description;
            PeriodText.Text =
                $"{voting.StartTime:G} — {voting.EndTime:G}";

            foreach (string option in voting.Options)
            {
                RadioButton rb = new RadioButton
                {
                    Content = option,
                    Tag = option, // string only
                    GroupName = "VotingOptions"
                };
                OptionsPanel.Children.Add(rb);
            }

        }

        private void Vote_Click(object sender, RoutedEventArgs e)
        {
            var selected = OptionsPanel.Children
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.IsChecked == true);

            if (selected == null)
            {
                MessageBox.Show("Please select an option.");
                return;
            }

            // Vote creation comes next
            MessageBox.Show("Vote successfully submitted!");

            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new VoterMainView();
        }
    }
}
