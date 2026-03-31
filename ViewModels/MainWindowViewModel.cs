using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
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
        public Dictionary<string, string> GoodHashes = new Dictionary<string, string>();
        public ObservableCollection<WidgetViewModel> Widgets { get; set; } = new();

        //global styling
        [ObservableProperty] private double _windowWidth = 192;
        [ObservableProperty] private double _windowHeight = 300;
        /*[ObservableProperty] private FontFamily _fontName = new FontFamily("Courier New, Monospace, Consolas");
        [ObservableProperty] private int _fontSize = 22;
        [ObservableProperty] private Color _fontColor = Colors.White;
        [ObservableProperty] private Color _bgColor = Colors.Black;
        public static List<FontFamily> SystemFonts { get; } = FontManager.Current.SystemFonts.OrderBy(f => f.Name).ToList();
        public List<FontFamily> AvailableFonts => SystemFonts;*/

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

            GoodHashes.Add("NTSC-J",    "CBB853BF911255C1D8EB27CD34FC7855A0DDA218");
            GoodHashes.Add("NTSC-U",    "47E103D8398CF5B7CBB42B95DF3A3C270691163B");
            GoodHashes.Add("PAL",       "27D9B4F30D39AF75075691344B7BDEEDBD32AC19");

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

            //global layout styling
            _windowHeight = layout.WindowHeight;
            _windowWidth = layout.WindowWidth;
            /*_fontName = layout.FontName;
            _fontSize = layout.FontSize;
            _fontColor = layout.FontColor;
            _bgColor = layout.BgColor;*/

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
                    //global
                    WindowWidth = _windowWidth,
                    WindowHeight = _windowHeight,
                    /*
                    FontName = _fontName.Name,
                    FontSize = _fontSize,
                    FontColor = _fontColor,
                    BgColor = _bgColor,*/

                    //widget
                    Widgets = Widgets.Select(w => new WidgetSettings
                    {
                        //GlobalStyle = w.GlobalStyle,
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
                await _logic.ReadMemory();
            }
        }

        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task Connect()
        {
            try
            {
                _logic.SnesSocket = new Usb2Snes.Usb2Snes();

                await _logic.SnesSocket.Connect();

                var devices = await _logic.SnesSocket.GetDeviceList();

                if (devices.Count > 0)
                {
                    await _logic.SnesSocket.Attach(devices[0]);
                    await _logic.SnesSocket.Name();

                    var infos = await _logic.SnesSocket.Info();
                    foreach (var info in infos)
                    {
                        Debug.WriteLine(info);
                    }

                    if (infos.Count > 0)
                    {
                        _logic.isAttached = true;
                        ConnectCommand.NotifyCanExecuteChanged();
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


        [RelayCommand]
        private async Task CheckRom()
        {
            try
            {
                var infos = await _logic.SnesSocket.Info();
                foreach (var info in infos)
                {
                    Debug.WriteLine(info);
                }

                //string remotePath = "/sd2snes/saves/Super Mario Kart (Japan).srm";
                string remotePath = "/4 Special Chip Games/Super Mario Kart (Japan).sfc";
                Debug.WriteLine($"trying to find {remotePath}.");
                byte[] data = await _logic.SnesSocket.GetFile(remotePath);

                if (data != null && data.Length > 0)
                {
                    await File.WriteAllBytesAsync("test.sfc", data);
                    Debug.WriteLine($"Success! Saved {data.Length} bytes.");
                }
                else
                {
                    Debug.WriteLine("File not found or empty.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
