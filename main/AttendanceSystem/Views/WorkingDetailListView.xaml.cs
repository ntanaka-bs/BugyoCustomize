using System.Windows;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// 出務者明細書画面のコードビハインドです。
    /// </summary>
    public partial class WorkingDetailListView : Window
    {
        public WorkingDetailListView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
