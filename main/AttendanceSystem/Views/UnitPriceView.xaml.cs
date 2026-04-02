using System.Windows;
using System.Windows.Input;
using AttendanceSystem.ViewModels;

namespace AttendanceSystem.Views
{
    public partial class UnitPriceView : Window
    {
        public UnitPriceView()
        {
            InitializeComponent(); if (this.DataContext is UnitPriceViewModel vm) { vm.RequestClose += () => this.Close(); }
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is UnitPriceViewModel vm)
            {
                if (e.Key == Key.F7 && vm.DeleteRowCommand.CanExecute(null))
                {
                    vm.DeleteRowCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F10 && vm.CancelCommand.CanExecute(null))
                {
                    vm.CancelCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.F12 && vm.F12Command.CanExecute(null))
                {
                    vm.F12Command.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
