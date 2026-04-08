using System.Windows;
using System.Windows.Input;
using AttendanceSystem.ViewModels;
using AttendanceSystem.Common;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック（コードビハインド）です。
    /// 主に UI に関するイベントハンドリングを記述します。
    /// </summary>
    public partial class DayKindView : Window
    {
        public DayKindView()
        {
            InitializeComponent();
            
            // ViewModel を DataContext に設定
            var vm = new DayKindRegistrationViewModel(); this.DataContext = vm; vm.RequestClose += () => this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show(MessageConfig.ConfirmClose, MessageConfig.TitleConfirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 画面の余白をクリックした際に、キーボードフォーカスを解除するためのイベントハンドラです。
        /// 変更内容を確定させたり、意図しない入力を防ぐ目的で使用します。
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 何もない場所をクリックしたらフォーカスを外す
            Keyboard.ClearFocus();
        }
    }
}