using System.Windows;
using AttendanceSystem.ViewModels;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// 時間帯区分登録用メイン画面
    /// </summary>
    public partial class TimeZoneView : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimeZoneView()
        {
            InitializeComponent();
            var vm = new TimeZoneViewModel();
            this.DataContext = vm;
            
            // ViewModelからの画面終了要求を購読
            vm.RequestClose += () => this.Close();
        }
    }
}
