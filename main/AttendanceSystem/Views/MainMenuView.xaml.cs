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

        private void OpenDayKindView_Click(object sender, RoutedEventArgs e)
        {
            var view = new DayKindView();
            view.ShowDialog();
        }

        private void OpenJobView_Click(object sender, RoutedEventArgs e)
        {
            var view = new JobView();
            view.ShowDialog();
        }

        private void OpenTimeZoneView_Click(object sender, RoutedEventArgs e)
        {
            var view = new TimeZoneView();
            view.ShowDialog();
        }

        private void OpenUnitPriceView_Click(object sender, RoutedEventArgs e)
        {
            var view = new UnitPriceView();
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
