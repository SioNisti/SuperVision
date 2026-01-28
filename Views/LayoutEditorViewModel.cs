using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SuperVision.ViewModels;
using SuperVision.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace SuperVision.ViewModels
{
    public partial class LayoutEditorViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainVm;

        public ObservableCollection<WidgetViewModel> Widgets => _mainVm.Widgets;

        public LayoutEditorViewModel(MainWindowViewModel mainVm)
        {
            _mainVm = mainVm;
        }

        [ObservableProperty]
        private WidgetTypeLookup? _selectedWidgetType;

        public record WidgetTypeLookup(string Name, Type Type);

        public List<WidgetTypeLookup> AvailableWidgetTypes => Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(WidgetViewModel)) && !t.IsAbstract)
            .Select(t => {
                var instance = (WidgetViewModel)Activator.CreateInstance(t)!;
                return new WidgetTypeLookup(instance.DisplayName, t);
            })
            .OrderBy(l => l.Name)
            .ToList();

        [RelayCommand]
        private void MoveWidgetUp(WidgetViewModel widget)
        {
            int index = Widgets.IndexOf(widget);
            if (index > 0)
            {
                Widgets.RemoveAt(index);
                Widgets.Insert(index - 1, widget);
                OnPropertyChanged(nameof(Widgets));
            }
        }

        [RelayCommand]
        private void MoveWidgetDown(WidgetViewModel widget)
        {
            int index = Widgets.IndexOf(widget);
            if (index < Widgets.Count - 1)
            {
                Widgets.RemoveAt(index);
                Widgets.Insert(index + 1, widget);
                OnPropertyChanged(nameof(Widgets));
            }
        }

        [RelayCommand]
        private void RemoveWidget(WidgetViewModel widget)
        {
            if (Widgets.Count < 2) return;

            int index = Widgets.IndexOf(widget);
            Widgets.RemoveAt(index);
            OnPropertyChanged(nameof(Widgets));
        }

        [RelayCommand]
        public async void AddSelectedWidget()
        {
            if (SelectedWidgetType == null) return;

            try
            {
                Type t = SelectedWidgetType.Type;

                var newWidget = (WidgetViewModel)Activator.CreateInstance(t)!;

                newWidget.BgColor = "black";
                newWidget.FontColor = "white";

                newWidget.RefreshDisplay();

                Widgets.Add(newWidget);
                _mainVm.Logic.ActiveWidgets.Add(newWidget);

                Debug.WriteLine($"Successfully added: {newWidget.DisplayName}");
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"Error adding widget.\n{ex.Message}",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error
                );
                await box.ShowAsync();
            }
        }

        [RelayCommand]
        private async void SaveLayout()
        {
            try
            {
                var settingsList = Widgets.Select(w => new WidgetSettings
                {
                    Type = w.WidgetType,
                    FontColor = w.FontColor,
                    BgColor = w.BgColor,
                    Variables = w.Variables.ToDictionary(v => v.Name, v => v.Value)
                }).ToList();

                string json = JsonSerializer.Serialize(settingsList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Globals.layoutPath, json);
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"Error saving layout.\n{ex.Message}",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error
                );
                await box.ShowAsync();
            }
        }

        [RelayCommand]
        private async Task EditWidget(WidgetViewModel widget)
        {
            if (widget == null) return;

            var editor = new WidgetEditor();

            editor.DataContext = widget;
            if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await editor.ShowDialog(desktop.MainWindow);
            }
            widget.RefreshDisplay();
        }
    }
}
