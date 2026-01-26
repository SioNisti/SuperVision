using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SuperVision.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperVision.Views
{
    public partial class GrindEditor : Window
    {
        public List<string> CoursesList { get; } = Globals.courses.ToList();
        private readonly MainWindowViewModel? _vm;

        public GrindEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public GrindEditor(MainWindowViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = this;
        }
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (Globals.isGrinding)
            {
                PopulateUI(Globals.grindPath);
                ActiveStatusText.IsVisible = true;

                StartButton.Content = "Stop";
                StartButton.Background = Brush.Parse("red");
            } else
            {
                Globals.grindPath = "";
                ActiveStatusText.IsVisible = false;

                StartButton.Content = "Start";
                StartButton.Background = Brush.Parse("green");
            }
        }

        private async void FindGrind(object? sender, RoutedEventArgs e)
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null) return;

            Globals.isGrinding = false;
            Globals.grindPath = "";
            ActiveStatusText.IsVisible = false;
            StartButton.Background = Brush.Parse("green");
            StartButton.Content = "Start";

            string startPath = Path.Combine(Globals.folder, "Grinds");

            if (!Directory.Exists(startPath)) Directory.CreateDirectory(startPath);

            var storageProvider = desktop.MainWindow.StorageProvider;
            var startLocation = await storageProvider.TryGetFolderFromPathAsync(startPath);

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Grind File",
                AllowMultiple = false,
                SuggestedStartLocation = startLocation,
                FileTypeFilter = new[] { new FilePickerFileType("Grind Files") { Patterns = new[] { "*.grind" } } }
            });

            if (files.Count >= 1)
            {
                string path = files[0].Path.LocalPath;

                Debug.WriteLine($"{path}");
                Globals.grindPath = path;

                PopulateUI(Globals.grindPath);
            }
        }

        private void SelectComboBoxItem(ComboBox box, string valueToFind)
        {
            foreach (ComboBoxItem item in box.Items)
            {
                if (item.Content?.ToString() == valueToFind)
                {
                    box.SelectedItem = item;
                    break;
                }
            }
        }

        private void PopulateUI(string path)
        {
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<GrindData>(json);
            if (data == null) return;

            CourseCombo.SelectedItem = data.Course;
            SelectComboBoxItem(RegionCombo, data.Region);
            SelectComboBoxItem(TypeCombo, data.GoalType);

            int totalCs = data.GoalTime;
            TimeM.Value = totalCs / 6000;
            int remainder = totalCs % 6000;
            TimeS.Value = remainder / 100;
            TimeMS.Value = remainder % 100;
        }

        private async void StartGrind(object? sender, RoutedEventArgs e)
        {
            if (Globals.isGrinding)
            {
                Globals.isGrinding = false;
                ActiveStatusText.IsVisible = false;
                StartButton.Background = Brush.Parse("green");
                StartButton.Content = "Start";
                return;
            }
            //new grind
            if (Globals.grindPath == "")
            {
                var regionItem = RegionCombo.SelectedItem as ComboBoxItem;
                var typeItem = TypeCombo.SelectedItem as ComboBoxItem;

                string REGION = regionItem?.Content?.ToString() ?? "NTSC";
                string TYPE = typeItem?.Content?.ToString() ?? "5lap";

                string COURSE = CourseCombo.SelectedItem?.ToString() ?? "MC1";

                int minutes = (int)(TimeM.Value ?? 0);
                int seconds = (int)(TimeS.Value ?? 0);
                int millis = (int)(TimeMS.Value ?? 0);

                string GOAL = $"{minutes}m{seconds:00}s{millis:00}ms";
                string goalString = $"{minutes}'{seconds:00}\"{millis:00}";

                string name = $"{REGION}-{COURSE}-{TYPE}-{GOAL}-{DateTime.Now.ToString("yyyyMMdd")}.grind";
                Globals.grindPath = Path.Combine(Globals.grindFolder, name);

                GrindData grind = new GrindData {
                    Region = REGION,
                    Course = COURSE,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    GoalTime = Globals.StrToCs(goalString),
                    GoalType = TYPE,
                };

                if (!File.Exists(Globals.grindPath))
                {
                    File.WriteAllText(Globals.grindPath, JsonSerializer.Serialize(grind, new JsonSerializerOptions { WriteIndented = true }));
                }
            } else
            {
                string json = File.ReadAllText(Globals.grindPath);
                var data = JsonSerializer.Deserialize<GrindData>(json);

                if (data.EndDate != null)
                {
                    Globals.isGrinding = false;
                    ActiveStatusText.IsVisible = false;
                    StartButton.Background = Brush.Parse("green");
                    Globals.grindPath = "";

                    var box = MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        "This grind has already been completed.",
                        ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Error
                    );
                    await box.ShowAsync();

                    return;
                }
            }

            Globals.isGrinding = true;
            ActiveStatusText.IsVisible = true;
            StartButton.Background = Brush.Parse("red");
            StartButton.Content = "Stop";
        }
    }
}