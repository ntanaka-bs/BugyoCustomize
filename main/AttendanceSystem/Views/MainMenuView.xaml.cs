using System.Windows;
using AttendanceSystem.Common;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// アプリケーションのメインメニュー画面を担当するクラスです。
    /// </summary>
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 日毎勤怠チェックリスト画面を開きます。
        /// </summary>
        private void OpenDailyCheckList_Click(object sender, RoutedEventArgs e)
        {
            var view = new DailyAttendanceCheckListView();
            view.ShowDialog();
        }

        /// <summary>
        /// マスタ管理メニュー画面を開きます。
        /// </summary>
        private void OpenMasterMenu_Click(object sender, RoutedEventArgs e)
        {
            var view = new MasterMenuView();
            view.ShowDialog();
        }

        /// <summary>
        /// 勤怠データ入力画面を開きます。
        /// </summary>
        private void OpenAttendanceRegistration_Click(object sender, RoutedEventArgs e)
        {
            var view = new AttendanceRegistrationView();
            view.ShowDialog();
        }

        /// <summary>
        /// 日割計算データ出力画面を開きます。
        /// </summary>
        private void OpenDailyRatOutput_Click(object sender, RoutedEventArgs e)
        {
            var view = new DailyRatOutputView();
            view.ShowDialog();
        }

        /// <summary>
        /// 有給一覧表画面を開きます。
        /// </summary>
        private void OpenPaidHolidayList_Click(object sender, RoutedEventArgs e)
        {
            var view = new PaidHolidayListView();
            view.ShowDialog();
        }

        /// <summary>
        /// ウィンドウを閉じる際の終了確認を行います。
        /// </summary>
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
