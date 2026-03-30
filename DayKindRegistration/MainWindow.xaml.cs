using System.Windows;
using System.Windows.Input;
using DayKindRegistration.ViewModel;

namespace DayKindRegistration
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック（コードビハインド）です。
    /// 主に UI に関するイベントハンドリングを記述します。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // ViewModel を DataContext に設定
            this.DataContext = new DayKindRegistrationViewModel();
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