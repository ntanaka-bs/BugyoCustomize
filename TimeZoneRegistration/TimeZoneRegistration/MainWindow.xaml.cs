using System.Windows;
using TimeZoneRegistration.ViewModel;

namespace TimeZoneRegistration
{
    /// <summary>
    /// 時間帯区分登録用メイン画面
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            this.DataContext = vm;
            
            // ViewModelからの画面終了要求を購読
            vm.RequestClose += () => this.Close();
        }
    }
}