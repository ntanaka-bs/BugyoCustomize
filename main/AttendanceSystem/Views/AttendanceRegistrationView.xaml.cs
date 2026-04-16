using System.Windows;

namespace AttendanceSystem.Views
{
    public partial class AttendanceRegistrationView : Window
    {
        public AttendanceRegistrationView()
        {
            InitializeComponent();
            DataContext = new ViewModels.AttendanceRegistrationViewModel();
        }
    }
}
