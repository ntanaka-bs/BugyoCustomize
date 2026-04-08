using System;
using System.Windows;
using AttendanceSystem.Common;

namespace AttendanceSystem.Views
{
    /// <summary>
    /// 各種マスタ登録画面への遷移を管理するマスタメニュー画面のクラスです。
    /// </summary>
    public partial class MasterMenuView : Window
    {
        public MasterMenuView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 日種類登録画面を開きます。
        /// </summary>
        private void OpenDayKindView_Click(object sender, RoutedEventArgs e)
        {
            var view = new DayKindView();
            view.ShowDialog();
        }

        /// <summary>
        /// 職種登録画面を開きます。
        /// </summary>
        private void OpenJobView_Click(object sender, RoutedEventArgs e)
        {
            var view = new JobView();
            view.ShowDialog();
        }

        /// <summary>
        /// 時間帯区分登録画面を開きます。
        /// </summary>
        private void OpenTimeZoneView_Click(object sender, RoutedEventArgs e)
        {
            var view = new TimeZoneView();
            view.ShowDialog();
        }

        /// <summary>
        /// 単価登録画面を開きます。
        /// </summary>
        private void OpenUnitPriceView_Click(object sender, RoutedEventArgs e)
        {
            var view = new UnitPriceView();
            view.ShowDialog();
        }
    }
}
