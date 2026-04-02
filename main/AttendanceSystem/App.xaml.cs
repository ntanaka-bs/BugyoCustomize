using System.Windows;
using AttendanceSystem.Common;

namespace AttendanceSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 起動時にデータベースの初期化（テーブル生成）を行う
            DatabaseInitializer.EnsureTablesCreated();
        }
    }
}
