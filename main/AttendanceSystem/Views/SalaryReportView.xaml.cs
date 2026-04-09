using System.Windows;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// 給与・賞与明細書画面のコードビハインドです。
    /// </summary>
    public partial class SalaryReportView : Window
    {
        public SalaryReportView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
