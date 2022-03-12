namespace RS.Fritz.Manager.UI
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Extensions.Logging;
    using RS.Fritz.Manager.API;


    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using CommunityToolkit.Mvvm.Messaging.Messages;
    using Microsoft.Extensions.Logging;
    using RS.Fritz.Manager.API;

    internal sealed class CaptureControlCaptureViewModel : FritzServiceViewModel
    {
        private readonly ICaptureControlService captureControlService;
        private int progBar_1_percent = 0;
        private Visibility progBar_1_Visibility = Visibility.Hidden;
        private string provisioningCode = string.Empty;
        private readonly DispatcherTimer animationTimer;

        public CaptureControlCaptureViewModel(DeviceLoginInfo deviceLoginInfo, ILogger logger, IFritzServiceOperationHandler fritzServiceOperationHandler, ICaptureControlService captureControlService)
           : base(deviceLoginInfo, logger, fritzServiceOperationHandler)
        {
            this.captureControlService = captureControlService;
            Start_1_Command = new AsyncRelayCommand(DoExecute_Start_1_Command_Async);
            Stop_1_Command = new AsyncRelayCommand(DoExecute_Stop_1_Command_Async);
            animationTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            animationTimer.Tick += AnimationTimer_Tick;
            
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            ProgBar_1_percent = (progBar_1_percent + 1) % 10;
        }

        public IAsyncRelayCommand Start_1_Command { get; }

        public IAsyncRelayCommand Stop_1_Command { get; }


        public int ProgBar_1_percent
        {
            get => progBar_1_percent;
            set => _ = SetProperty(ref progBar_1_percent, value);
        }

        public Visibility ProgBar_1_Visibility
        {
            get => progBar_1_Visibility;
            set => _ = SetProperty(ref progBar_1_Visibility, value);
        }

    //get => hostsGetGenericHostEntryResponse; set { _ = SetProperty(ref hostsGetGenericHostEntryResponse, value);


    protected override async Task DoExecuteDefaultCommandAsync()
        {
            // do nothing
            await Task.Delay(100);
        }

        protected override void FritzServiceViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.FritzServiceViewModelPropertyChanged(sender, e);
            /*
            switch (e.PropertyName)
            {
                case nameof(ProvisioningCode):
                    {
                        UpdateCanExecuteDefaultCommand();
                        break;
                    }
            }
            */
        }

        private async Task DoExecute_Start_1_Command_Async()
        {
            string sid = await GetSidAsync();
            const string iface = "2-1";
            const string capturePath = $"/cgi-bin/capture_notimeout";
            const string Scheme = "http";
            const string Host = "fritz.box";
            string query = FormattableString.Invariant($"sid={sid}&capture=Start&snaplen=1600&ifaceorminor={iface}");
            Uri captureUri = new Uri(FormattableString.Invariant($"{Scheme}://{Host}{capturePath}?{query}"));

            ProgBar_1_Visibility = Visibility.Visible;
            animationTimer.Start();

            //var theResult = await captureControlService.GetStartCaptureResponseAsync(captureUri);
            //var theResult = await captureControlService.GetStartCaptureResponseStreamAsync(captureUri);
            var theResult = await captureControlService.GetStartCaptureResponseSocketAsync(Scheme, Host, capturePath, query);

            progBar_1_percent = 0;
            animationTimer.Stop();
            ProgBar_1_Visibility = Visibility.Hidden;

        }

        private async Task DoExecute_Stop_1_Command_Async()
        {
            string sid = await GetSidAsync();
            string iface = "eth_udma0";
            string capturePath = $"/cgi-bin/capture_notimeout?";
            Uri captureUri = new Uri(FormattableString.Invariant($"http://fritz.box{capturePath}iface={iface}&minor=1&type=2&capture=Stop&sid={sid}&useajax=1&xhr=1&t1646748233367=nocache"));

            var theResult = await captureControlService.GetStopCaptureResponseAsync(captureUri);
        }

        private async Task<string> GetSidAsync()
        {
            HostsGetHostListPathResponse newHostsGetHostListPathResponse = await FritzServiceOperationHandler.GetHostsGetHostListPathAsync();
            string hostListPath = newHostsGetHostListPathResponse.HostListPath;
            string returnString = hostListPath.Substring((hostListPath.LastIndexOf("sid=") != -1) ? hostListPath.LastIndexOf("sid=") : hostListPath.Length - 1);
            returnString = returnString.Length >= 4 ? returnString.Remove(0, 4) : string.Empty;
            return returnString;
        }

    }
}