using System.Windows;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// 給与連携データ出力画面のViewクラス
    /// </summary>
    public partial class SalaryOutputView : Window
    {
        public SalaryOutputView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
