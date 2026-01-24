using EVotingSystem.Models;
using EVotingSystem.Persistence;
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

            //Calendar setup

            for (int h = 0; h < 24; h++)
            {
                StartHourBox.Items.Add(h.ToString("D2"));
                EndHourBox.Items.Add(h.ToString("D2"));
            }

            for (int m = 0; m < 60; m++)
            {
                StartMinuteBox.Items.Add(m.ToString("D2"));
                EndMinuteBox.Items.Add(m.ToString("D2"));
            }

            var now = DateTime.Now;
            var end = now.AddMinutes(10);

            StartDatePicker.SelectedDate = now.Date;
            EndDatePicker.SelectedDate = end.Date;

            StartHourBox.SelectedIndex = now.Hour;
            StartMinuteBox.SelectedIndex = now.Minute;

            EndHourBox.SelectedIndex = end.Hour;
            EndMinuteBox.SelectedIndex = end.Minute;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainContent.Content = new OrganizerMainView();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Naslov je obavezan.");
                return;
            }

            if (!StartDatePicker.SelectedDate.HasValue ||
                !EndDatePicker.SelectedDate.HasValue ||
                StartHourBox.SelectedItem == null ||
                StartMinuteBox.SelectedItem == null ||
                EndHourBox.SelectedItem == null ||
                EndMinuteBox.SelectedItem == null)
            {
                MessageBox.Show("Morate uneti početak i kraj glasanja.");
                return;
            }

            DateTime startTime = StartDatePicker.SelectedDate.Value
                .AddHours(int.Parse(StartHourBox.SelectedItem.ToString()!))
                .AddMinutes(int.Parse(StartMinuteBox.SelectedItem.ToString()!));

            DateTime endTime = EndDatePicker.SelectedDate.Value
                .AddHours(int.Parse(EndHourBox.SelectedItem.ToString()!))
                .AddMinutes(int.Parse(EndMinuteBox.SelectedItem.ToString()!));

            if (endTime <= startTime)
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
                StartTime = startTime,
                EndTime = endTime,
                Options = options
            };

            VotingRepository.AddVoting(voting);

            MessageBox.Show(
                $"Glasanje \"{voting.Title}\" kreirano.\n" +
                $"Početak: {startTime}\nKraj: {endTime}");
        }
    }
}
