using System.Windows;
using System.Windows.Input;
using UnitPriceRegistration.ViewModels;

namespace UnitPriceRegistration
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel vm)
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