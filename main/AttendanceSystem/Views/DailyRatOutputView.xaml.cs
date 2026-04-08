using System.Windows;

namespace AttendanceSystem.Views
{
    public partial class DailyRatOutputView : Window
    {
        public DailyRatOutputView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
