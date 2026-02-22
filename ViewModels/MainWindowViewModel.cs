using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SuperVision.Services;
using SuperVision.ViewModels;
using SuperVision.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SuperVision.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainLogic Logic => _logic;
        private readonly MainLogic _logic = new MainLogic();
        public ObservableCollection<WidgetViewModel> Widgets { get; set; } = new();
        [ObservableProperty] private double _windowWidth = 192; 
        [ObservableProperty] private double _windowHeight = 300;
        public MainWindowViewModel()
        {
            //initialize the sessiondata variable
            foreach (var course in Globals.courses) { Globals.sessionData[course] = new SessionData(); }

            //load all prefix codes
            Globals.loadPrefixes();

            //load layout
            _logic = new MainLogic();
            _logic.CheckJson();
            LoadLayout();


            //add the "logger" to the program. this is the thing that saves the data.json
            var logger = new AttemptDataService();
            _logic.ActiveWidgets.Add(logger);

            Task.Run(() => RunMemoryLoop());
        }

        public void LoadLayout()
        {
            string json = File.ReadAllText(Globals.layoutPath);
            var layout  = JsonSerializer.Deserialize<LayoutSaveData>(json);
            var list = layout.Widgets.ToList();

            _windowHeight = layout.WindowHeight;
            _windowWidth = layout.WindowWidth;

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

                    widget.ApplySettings(item);

                    Widgets.Add(widget);
                    _logic.ActiveWidgets.Add(widget);
                }
            }
        }
        public async Task SaveLayoutAsync()
        {
            try
            {
                var layoutData = new LayoutSaveData
                {
                    WindowWidth = _windowWidth,
                    WindowHeight = _windowHeight,
                    
                    FontName = $"{DateTime.Now.ToString()}",
                    FontSize = (int)DateTime.Now.Ticks,
                    FontColor = Colors.Cyan,
                    BgColor = Colors.Cyan,

                    Widgets = Widgets.Select(w => new WidgetSettings
                    {
                        Type = w.WidgetType,
                        FontName = w.FontName.Name,
                        FontSize = w.FontSize,
                        FontColor = w.FontColor,
                        BgColor = w.BgColor,
                        Variables = w.Variables.ToDictionary(v => v.Name, v => v.Value)
                    }).ToList()
                };

                string json = JsonSerializer.Serialize(layoutData, new JsonSerializerOptions { WriteIndented = true });
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

        private async Task RunMemoryLoop()
        {
            while (true)
            {
                _logic.ReadMemory();

                await Task.Delay(16);
            }
        }

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task Connect()
        {
            try
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
            } catch (TaskCanceledException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"QUSB2SNES Connection Error.",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error
                );
                await box.ShowAsync();
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
            var vm = new LayoutEditorViewModel(this);
            var win = new LayoutEditor(vm);
            win.Show();
        }

        [RelayCommand]
        private void EditGrind()
        {
            var editor = new GrindEditor(this);
            editor.Show();
        }

        [RelayCommand]
        private async void OpenSaveDir()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Globals.folder,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", Globals.folder);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", Globals.folder);
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    $"Error opening directory.\n{ex.Message}",
                    ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Error
                );
                await box.ShowAsync();
            }
        }

        [RelayCommand]
        private void SetAllTime()
        {
            Globals.currentComparison = "All Time";
        }

        [RelayCommand]
        private void SetSession()
        {
            Globals.currentComparison = "Session";
        }

        [RelayCommand]
        private void SetGrind()
        {
            Globals.currentComparison = "Grind";
        }
    }
}
