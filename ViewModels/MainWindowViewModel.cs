using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperVision.Services;
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
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly MainLogic _logic;

        public MainWindowViewModel()
        {

            _logic = new MainLogic();
            _logic.CheckJson();
            LoadLayout();

            foreach (var course in Globals.courses)
            {
                Globals.sessionData[course] = new SessionData();
            }

            //add the "logger" to the program. this is the thing that saves the data.json
            var logger = new AttemptDataService();
            _logic.ActiveWidgets.Add(logger);
            //add the "grinder" to the program. this is the thing that saves the grinding data, if a grind is active
            var grinder = new GrindDataService();
            _logic.ActiveWidgets.Add(grinder);

            Task.Run(() => RunMemoryLoop());
        }
        public ObservableCollection<WidgetViewModel> Widgets { get; set; } = new();

        public void LoadLayout()
        {
            string json = File.ReadAllText(Globals.layoutPath);
            var list = JsonSerializer.Deserialize<List<WidgetSettings>>(json);

            Widgets.Clear();
            _logic.ActiveWidgets.Clear();

            var widgetTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(WidgetViewModel)) && !t.IsAbstract);

            foreach (var item in list)
            {
                var match = widgetTypes.FirstOrDefault(t =>
                {
                    var instance = (IWidget?)Activator.CreateInstance(t);
                    return instance?.WidgetType == item.Type;
                });

                if (match != null)
                {
                    var widget = (WidgetViewModel)Activator.CreateInstance(match)!;

                    widget.BgColor = item.BgColor;
                    widget.FontColor = item.FontColor;

                    Widgets.Add(widget);
                    _logic.ActiveWidgets.Add(widget);
                }
            }
        }

        private async Task RunMemoryLoop()
        {
            while (true)
            {
                _logic.ReadMemory();

                await Task.Delay(16);
            }
        }

        //[RelayCommand]
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private void Connect()
        {
            _logic.Snessocket = new Snessocket();
            _logic.Snessocket.wsConnect();

            var socket = _logic.Snessocket;

            if (socket != null && socket.Connected() && !_logic.isAttached)
            {
                string[] devices = socket.GetDevices();

                if (devices.Length > 0)
                {
                    socket.Attach(devices[0]);
                    socket.Name("SuperVision");

                    string[] infos = socket.GetInfo();
                    foreach (string info in infos)
                    {
                        Debug.WriteLine(info);
                    }

                    if (infos.Length > 0)
                    {
                        _logic.isAttached = true;
                        ConnectCommand.NotifyCanExecuteChanged();
                    }
                }
            }
        }
        private bool CanConnect()
        {
            return !_logic.isAttached;
        }

        [ObservableProperty]
        private bool _IsUsingPAL = false;
        partial void OnIsUsingPALChanged(bool value)
        {
            if (_logic != null)
            {
                Globals.currentRegion = value ? "PAL" : "NTSC";
            }
        }

        [RelayCommand]
        private void Exit()
        {
            Environment.Exit(0);
        }

        [RelayCommand]
        private void EditLayout()
        {
            var editor = new LayoutEditor(this);
            editor.Show();
        }

        [RelayCommand]
        private void EditGrind()
        {
            var editor = new GrindEditor(this);
            editor.Show();
        }

        //LAYOUT EDITOR
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
        public void AddSelectedWidget()
        {
            if (SelectedWidgetType == null) return; 

            try
            {
                Type t = SelectedWidgetType.Type;

                var newWidget = (WidgetViewModel)Activator.CreateInstance(t)!;

                newWidget.BgColor = "black";
                newWidget.FontColor = "white";

                Widgets.Add(newWidget);
                _logic.ActiveWidgets.Add(newWidget);

                Debug.WriteLine($"Successfully added: {newWidget.DisplayName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding widget: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SaveLayout()
        {
            var settingsList = Widgets.Select(w => new WidgetSettings
            {
                Type = w.WidgetType,
                FontColor = w.FontColor,
                BgColor = w.BgColor
            }).ToList();

            string json = JsonSerializer.Serialize(settingsList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Globals.layoutPath, json);
        }
    }
}
