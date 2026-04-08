using System.Windows;
using AttendanceSystem.Common;

namespace AttendanceSystem.Views
{
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        private void OpenDailyCheckList_Click(object sender, RoutedEventArgs e)
        {
            var view = new DailyAttendanceCheckListView();
            view.ShowDialog();
        }

        private void OpenMasterMenu_Click(object sender, RoutedEventArgs e)
        {
            var view = new MasterMenuView();
            view.ShowDialog();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show(MessageConfig.ConfirmClose, MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
