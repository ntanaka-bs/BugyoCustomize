using System;
using System.Windows;

namespace AttendanceSystem.Views
{
    public partial class DailyAttendanceCheckListView : Window
    {
        public DailyAttendanceCheckListView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
