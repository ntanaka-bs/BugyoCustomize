using System.Windows;

namespace AttendanceSystem.Views
{
    public partial class AttendanceRegistrationView : Window
    {
        public AttendanceRegistrationView()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
