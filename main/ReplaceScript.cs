using System;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        var utf8Out = new UTF8Encoding(true); // write with BOM
        string target = @"C:\Users\aokada\Documents\MyProject\Work\04_阪南ビジネスマシン\src\BugyoCustomize\main\AttendanceSystem";
        
        void ReplaceInFile(string path, string oldVal, string newVal) {
            if (File.Exists(path)) {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                byte[] bytes = File.ReadAllBytes(path);
                bool hasUtf8Bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                Encoding enc = hasUtf8Bom ? Encoding.UTF8 : Encoding.GetEncoding(932);
                
                string content = File.ReadAllText(path, enc);
                if (content.Contains(oldVal)) {
                    content = content.Replace(oldVal, newVal);
                    File.WriteAllText(path, content, enc);
                }
            }
        }

        foreach (string file in Directory.GetFiles(Path.Combine(target, "Models"), "*.cs", SearchOption.AllDirectories)) {
            ReplaceInFile(file, "DayKindRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "JobRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "TimeZoneRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "UnitPriceRegistration.Models", "AttendanceSystem.Models");
            ReplaceInFile(file, "namespace DayKindRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace JobRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace TimeZoneRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace UnitPriceRegistration", "namespace AttendanceSystem.Views");
        }
        foreach (string file in Directory.GetFiles(Path.Combine(target, "ViewModels"), "*.cs", SearchOption.AllDirectories)) {
            ReplaceInFile(file, "DayKindRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "JobRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "TimeZoneRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "UnitPriceRegistration.ViewModels", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "namespace DayKindRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace JobRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace TimeZoneRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace UnitPriceRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "DayKindRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "JobRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "TimeZoneRegistration.Model", "AttendanceSystem.Models");
            ReplaceInFile(file, "UnitPriceRegistration.Models", "AttendanceSystem.Models");
            ReplaceInFile(file, "namespace AttendanceSystem.Views.ViewModel", "namespace AttendanceSystem.ViewModels");
            ReplaceInFile(file, "namespace AttendanceSystem.Views.ViewModels", "namespace AttendanceSystem.ViewModels");
        }
        
        foreach (string file in Directory.GetFiles(Path.Combine(target, "Views"), "*.cs", SearchOption.AllDirectories)) {
            ReplaceInFile(file, "DayKindRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "JobRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "TimeZoneRegistration.ViewModel", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "UnitPriceRegistration.ViewModels", "AttendanceSystem.ViewModels");
            ReplaceInFile(file, "namespace DayKindRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace JobRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace TimeZoneRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "namespace UnitPriceRegistration", "namespace AttendanceSystem.Views");
            ReplaceInFile(file, "using DayKindRegistration.Model;", "using AttendanceSystem.Models;");
        }

        foreach (string file in Directory.GetFiles(Path.Combine(target, "Views"), "*.xaml", SearchOption.AllDirectories)) {
            ReplaceInFile(file, "clr-namespace:DayKindRegistration"", "clr-namespace:AttendanceSystem.Views"");
            ReplaceInFile(file, "clr-namespace:JobRegistration"", "clr-namespace:AttendanceSystem.Views"");
            ReplaceInFile(file, "clr-namespace:TimeZoneRegistration"", "clr-namespace:AttendanceSystem.Views"");
            ReplaceInFile(file, "clr-namespace:UnitPriceRegistration"", "clr-namespace:AttendanceSystem.Views"");
            
            ReplaceInFile(file, "clr-namespace:DayKindRegistration.ViewModel"", "clr-namespace:AttendanceSystem.ViewModels"");
            ReplaceInFile(file, "clr-namespace:JobRegistration.ViewModel"", "clr-namespace:AttendanceSystem.ViewModels"");
            ReplaceInFile(file, "clr-namespace:TimeZoneRegistration.ViewModel"", "clr-namespace:AttendanceSystem.ViewModels"");
            ReplaceInFile(file, "clr-namespace:UnitPriceRegistration.ViewModels"", "clr-namespace:AttendanceSystem.ViewModels"");
        }

        ReplaceInFile(Path.Combine(target, "Views\DayKindView.xaml"), "DayKindRegistration.MainWindow"", "AttendanceSystem.Views.DayKindView"");
        ReplaceInFile(Path.Combine(target, "Views\DayKindView.xaml.cs"), "MainWindow : Window", "DayKindView : Window");
        ReplaceInFile(Path.Combine(target, "Views\DayKindView.xaml.cs"), "public MainWindow()", "public DayKindView()");

        ReplaceInFile(Path.Combine(target, "Views\JobView.xaml"), "JobRegistration.MainWindow"", "AttendanceSystem.Views.JobView"");
        ReplaceInFile(Path.Combine(target, "Views\JobView.xaml.cs"), "MainWindow : Window", "JobView : Window");
        ReplaceInFile(Path.Combine(target, "Views\JobView.xaml.cs"), "public MainWindow()", "public JobView()");

        ReplaceInFile(Path.Combine(target, "Views\TimeZoneView.xaml"), "TimeZoneRegistration.MainWindow"", "AttendanceSystem.Views.TimeZoneView"");
        ReplaceInFile(Path.Combine(target, "Views\TimeZoneView.xaml.cs"), "MainWindow : Window", "TimeZoneView : Window");
        ReplaceInFile(Path.Combine(target, "Views\TimeZoneView.xaml.cs"), "public MainWindow()", "public TimeZoneView()");

        ReplaceInFile(Path.Combine(target, "Views\UnitPriceView.xaml"), "UnitPriceRegistration.MainWindow"", "AttendanceSystem.Views.UnitPriceView"");
        ReplaceInFile(Path.Combine(target, "Views\UnitPriceView.xaml.cs"), "MainWindow : Window", "UnitPriceView : Window");
        ReplaceInFile(Path.Combine(target, "Views\UnitPriceView.xaml.cs"), "public MainWindow()", "public UnitPriceView()");

        ReplaceInFile(Path.Combine(target, "ViewModels\JobViewModel.cs"), "class MainViewModel", "class JobViewModel");
        ReplaceInFile(Path.Combine(target, "ViewModels\JobViewModel.cs"), "public MainViewModel(", "public JobViewModel(");
        ReplaceInFile(Path.Combine(target, "ViewModels\TimeZoneViewModel.cs"), "class MainViewModel", "class TimeZoneViewModel");
        ReplaceInFile(Path.Combine(target, "ViewModels\TimeZoneViewModel.cs"), "public MainViewModel(", "public TimeZoneViewModel(");
        ReplaceInFile(Path.Combine(target, "ViewModels\UnitPriceViewModel.cs"), "class MainViewModel", "class UnitPriceViewModel");
        ReplaceInFile(Path.Combine(target, "ViewModels\UnitPriceViewModel.cs"), "public MainViewModel(", "public UnitPriceViewModel(");

        ReplaceInFile(Path.Combine(target, "Views\JobView.xaml"), "<viewmodel:MainViewModel", "<viewmodel:JobViewModel");
        ReplaceInFile(Path.Combine(target, "Views\TimeZoneView.xaml"), "<vm:MainViewModel", "<vm:TimeZoneViewModel");
        ReplaceInFile(Path.Combine(target, "Views\UnitPriceView.xaml"), "<vm:MainViewModel", "<vm:UnitPriceViewModel");
        
        Console.WriteLine("Replacement by C# complete");
    }
}