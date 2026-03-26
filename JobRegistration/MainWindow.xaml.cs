using System.Windows;
using JobRegistration.ViewModel;

namespace JobRegistration
{
    /// <summary>
    /// メイン画面 (MainWindow) のコードビハインドです。
    /// UI 要素の初期化と、ViewModel の接続を行います。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var vm = new MainViewModel();
            DataContext = vm;

            // ViewModel からの「閉じる」要求を購読
            vm.RequestClose += () => this.Close();
        }
    }
}
