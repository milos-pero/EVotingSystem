using EVotingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EVotingSystem.Views
{
    public partial class CreateVotingView : UserControl
    {
        public CreateVotingView()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Naslov je obavezan.");
                return;
            }

            if (!StartDatePicker.SelectedDate.HasValue ||
                !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Morate uneti početak i kraj glasanja.");
                return;
            }

            if (EndDatePicker.SelectedDate <= StartDatePicker.SelectedDate)
            {
                MessageBox.Show("Kraj glasanja mora biti posle početka.");
                return;
            }

            var options = new List<string>
            {
                Option1TextBox.Text,
                Option2TextBox.Text,
                Option3TextBox.Text,
                Option4TextBox.Text,
                Option5TextBox.Text
            }
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToList();

            if (options.Count < 2 || options.Count > 5)
            {
                MessageBox.Show("Glasanje mora imati između 2 i 5 opcija.");
                return;
            }

            var voting = new Voting
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                StartTime = StartDatePicker.SelectedDate.Value,
                EndTime = EndDatePicker.SelectedDate.Value,
                Options = options
            };

            // For now: just confirm creation
            MessageBox.Show(
                $"Glasanje \"{voting.Title}\" uspešno kreirano sa {voting.Options.Count} opcija.");

            // Later:
            // VotingRepository.Add(voting);
            // Navigate back to organizer main view
        }
    }
}
