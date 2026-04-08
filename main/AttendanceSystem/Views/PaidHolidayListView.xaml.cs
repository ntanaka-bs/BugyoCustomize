using System.Windows;

namespace AttendanceSystem.Views
{
    public partial class PaidHolidayListView : Window
    {
        public PaidHolidayListView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
