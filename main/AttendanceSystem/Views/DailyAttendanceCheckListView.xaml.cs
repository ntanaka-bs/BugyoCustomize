using System.Windows;
using AttendanceSystem.ViewModels;

namespace AttendanceSystem.Views
{
    public partial class DailyAttendanceCheckListView : Window
    {
        public DailyAttendanceCheckListView()
        {
            InitializeComponent();
            this.DataContext = new DailyAttendanceCheckListViewModel();
        }
    }
}
