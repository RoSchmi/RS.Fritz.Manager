namespace RS.Fritz.Manager.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Extensions.Logging;
    using RS.Fritz.Manager.API;

    internal sealed class CaptureControlCaptureViewModel : FritzServiceViewModel
    {
        private const int TimerTickIntervalMs = 200;
        private readonly ICaptureControlService captureControlService;
        private readonly DispatcherTimer animationTimer;
        private int progBarPercent01 = 0;
        private Visibility progBarVisibility01 = Visibility.Hidden;
        private int captureTimeLimitMinutes;
        private string filenamePrefix = "FritzboxCapture";
        private string selectedTargetFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string targetFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private int milliSecondsCaptured = 0;
        private ObservableCollection<string> targetFolders = new ObservableCollection<string> { string.Empty };
        private const string CapturePath = $"/cgi-bin/capture_notimeout";
        private const string Scheme = "http";
        private const string Host = "fritz.box";

        public CaptureControlCaptureViewModel(DeviceLoginInfo deviceLoginInfo, ILogger logger, IFritzServiceOperationHandler fritzServiceOperationHandler, ICaptureControlService captureControlService)
           : base(deviceLoginInfo, logger, fritzServiceOperationHandler)
        {
            this.captureControlService = captureControlService;
            Start_1_Command = new AsyncRelayCommand(DoExecute_Start_1_Command_Async);
            Stop_1_Command = new AsyncRelayCommand(DoExecute_Stop_1_Command_Async);
            animationTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(TimerTickIntervalMs)
            };
            animationTimer.Tick += AnimationTimer_Tick;
            CaptureTimeLimitMinutes = 5;
            TargetFolders = new ObservableCollection<string>()
            {
                "Downloads",
                "Documents",
                "Desktop"
            };
            SelectedTargetFolder = TargetFolders[0];
        }

        public int ProgBarPercent01
        {
            get => progBarPercent01;
            set => _ = SetProperty(ref progBarPercent01, value);
        }

        public Visibility ProgBarVisibility01
        {
            get => progBarVisibility01;
            set => _ = SetProperty(ref progBarVisibility01, value);
        }

        public int CaptureTimeLimitMinutes
        {
            get => captureTimeLimitMinutes;
            set
            {
                value = value == 100 ? 9999 : value;
                SetProperty(ref captureTimeLimitMinutes, value);
            }
        }

        public string FilenamePrefix
        {
            get => filenamePrefix;
            set => _ = SetProperty(ref filenamePrefix, value);
        }

        public ObservableCollection<string> TargetFolders { get => targetFolders; set => _ = SetProperty(ref targetFolders, value); }

        public string SelectedTargetFolder
        {
            get => selectedTargetFolder;
            set
            {
                SetProperty(ref selectedTargetFolder, value);
                targetFolder = UpdateFolderPath(value);
            }
        }

        public IAsyncRelayCommand Start_1_Command { get; }

        public IAsyncRelayCommand Stop_1_Command { get; }

        protected override async Task DoExecuteDefaultCommandAsync()
        {
            // do nothing
            await Task.Delay(1);
        }

        private async Task DoExecute_Start_1_Command_Async()
        {
            string sid = await GetSidAsync();
            const string iface = "2-1";
            string query = FormattableString.Invariant($"sid={sid}&capture=Start&snaplen=1600&ifaceorminor={iface}");
            Uri captureUri = new Uri(FormattableString.Invariant($"{Scheme}://{Host}{CapturePath}?{query}"));

            if (await InvalidTargetPath(targetFolder, FilenamePrefix))
            {
                return;
            }

            ProgBarVisibility01 = Visibility.Visible;
            animationTimer.Start();
            milliSecondsCaptured = 0;

            await captureControlService.GetStartCaptureResponseAsync(captureUri, targetFolder, filenamePrefix);
            //var theResult = await captureControlService.GetStartCaptureResponseStreamAsync(captureUri);
            //var theResult = await captureControlService.GetStartCaptureResponseSocketAsync(Scheme, Host, capturePath, query, CaptureTimeLimitMinutes);

            progBarPercent01 = 0;
            animationTimer.Stop();
            ProgBarVisibility01 = Visibility.Hidden;
        }

        private async Task DoExecute_Stop_1_Command_Async()
        {
            string sid = await GetSidAsync();
            string iface = "eth_udma0";
            string timeString20 = DateTime.UtcNow.Ticks.ToString("D20");
            string timeId = FormattableString.Invariant($"t{timeString20.Substring(timeString20.Length - 13)}");
            Uri captureUri = new Uri(FormattableString.Invariant($"{Scheme}://{Host}{CapturePath}?iface={iface}&minor=1&type=2&capture=Stop&sid={sid}&useajax=1&xhr=1&{timeId}=nocache"));

            //Uri captureUri = new Uri(FormattableString.Invariant($"{Scheme}://{Host}{CapturePath}?iface={iface}&minor=1&type=2&capture=Stop&sid={sid}&useajax=1&xhr=1&t1646748233367=nocache"));

            await captureControlService.GetStopCaptureResponseAsync(captureUri);
        }

        private async void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            ProgBarPercent01 = (progBarPercent01 + 1) % 10;
            milliSecondsCaptured += TimerTickIntervalMs;
            if (milliSecondsCaptured > CaptureTimeLimitMinutes * 60 * 1000)
            {
                animationTimer.Stop();

                await DoExecute_Stop_1_Command_Async();
            }
        }

        private async Task<bool> InvalidTargetPath(string targetFolder, string filenamePrefix)
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

            bool invalidFilename = false;

            if (filenamePrefix.IndexOfAny(invalidFileNameChars) != -1)
            {
                invalidFilename = true;
            }

            targetFolder = invalidFilename ? "ThisIsAnInvalidPath::::" : targetFolder;

            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                return false;
            }
            catch
            {
                WeakReferenceMessenger.Default.Send(new UserMessageValueChangedMessage(new UserMessage("Invalid character in TargetFolder or FilenamePrefix")));
                await Task.Delay(3000);
                WeakReferenceMessenger.Default.Send(new UserMessageValueChangedMessage(new UserMessage(string.Empty)));
                return true;
            }
        }

        private async Task<string> GetSidAsync()
        {
            HostsGetHostListPathResponse newHostsGetHostListPathResponse = await FritzServiceOperationHandler.GetHostsGetHostListPathAsync();
            string hostListPath = newHostsGetHostListPathResponse.HostListPath;
            string returnString = hostListPath.Substring((hostListPath.LastIndexOf("sid=") != -1) ? hostListPath.LastIndexOf("sid=") : hostListPath.Length - 1);
            returnString = returnString.Length >= 4 ? returnString.Remove(0, 4) : string.Empty;
            return returnString;
        }

        private string UpdateFolderPath(string folderPath)
        {
            string returnPath = string.Empty;
            switch (folderPath)
            {
                case "Documents":
                    {
                        returnPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        break;
                    }

                case "Downloads":
                    {
                        returnPath = SpecialFolder.Downloads;
                        break;
                    }

                case "Desktop":
                    {
                        returnPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        break;
                    }

                default:
                    {
                        returnPath = FormattableString.Invariant($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\{folderPath}");
                        break;
                    }
            }

            return returnPath;
        }
    }
}