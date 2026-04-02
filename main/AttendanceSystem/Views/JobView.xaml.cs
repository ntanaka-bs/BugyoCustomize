using System.Windows;
using AttendanceSystem.ViewModels;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// メイン画面 (MainWindow) のコードビハインドです。
    /// UI 要素の初期化と、ViewModel の接続を行います。
    /// </summary>
    public partial class JobView : Window
    {
        public JobView()
        {
            InitializeComponent();
            
            var vm = new JobViewModel();
            DataContext = vm;

            // ViewModel からの「閉じる」要求を購読
            vm.RequestClose += () => this.Close();
        }
    }
}
