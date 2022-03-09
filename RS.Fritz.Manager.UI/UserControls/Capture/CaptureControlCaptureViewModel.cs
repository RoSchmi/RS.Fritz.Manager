namespace RS.Fritz.Manager.UI
{
    
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Extensions.Logging;
    using RS.Fritz.Manager.API;

    internal sealed class CaptureControlCaptureViewModel : FritzServiceViewModel
    {
        private readonly ICaptureControlService captureControlService;
        private string provisioningCode = string.Empty;

        public CaptureControlCaptureViewModel(DeviceLoginInfo deviceLoginInfo, ILogger logger, IFritzServiceOperationHandler fritzServiceOperationHandler, ICaptureControlService captureControlService)
           : base(deviceLoginInfo, logger, fritzServiceOperationHandler)
        {
            this.captureControlService = captureControlService;
            Start_1_Command = new AsyncRelayCommand(DoExecute_Start_1_Command_Async);
            Stop_1_Command = new AsyncRelayCommand(DoExecute_Stop_1_Command_Async);
        }

        public IAsyncRelayCommand Start_1_Command { get; }

        public IAsyncRelayCommand Stop_1_Command { get; }

        public string ProvisioningCode
        {
            get => provisioningCode;
            set
            {
                if (SetProperty(ref provisioningCode, value))
                    DefaultCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task DoExecute_Stop_1_Command_Async()
        {
            string sid = await GetSidAsync();
            string iface = "eth_udma0";
            string capturePath = $"/cgi-bin/capture_notimeout?";
            Uri captureUri = new Uri(FormattableString.Invariant($"http://fritz.box{capturePath}iface={iface}&minor=1&type=2&capture=Stop&sid={sid}&useajax=1&xhr=1&t1646748233367=nocache"));

            var theResult = await captureControlService.GetStopCaptureResponseAsync(captureUri);
        }

        private async Task DoExecute_Start_1_Command_Async()
        {
            string sid = await GetSidAsync();
            string iface = "2-1";
            string capturePath = $"/cgi-bin/capture_notimeout?";
            Uri captureUri = new Uri(FormattableString.Invariant($"http://fritz.box{capturePath}sid={sid}&capture=Start&snaplen=1600&ifaceorminor={iface}"));

            var theResult = await captureControlService.GetStartCaptureResponseAsync(captureUri);
        }


        protected override async Task DoExecuteDefaultCommandAsync()
        {
            await Task.Delay(100);
        }

        private async Task<string> GetSidAsync()
        {
            HostsGetHostListPathResponse newHostsGetHostListPathResponse = await FritzServiceOperationHandler.GetHostsGetHostListPathAsync();
            string hostListPath = newHostsGetHostListPathResponse.HostListPath;
            string returnString = hostListPath.Substring((hostListPath.LastIndexOf("sid=") != -1) ? hostListPath.LastIndexOf("sid=") : hostListPath.Length - 1);
            returnString = returnString.Length >= 4 ? returnString.Remove(0, 4) : string.Empty;
            return returnString;
        }

        private async Task ExecuteStart_1_CommandAsync()
        {

        }

        private async Task GetCaptureControlMemberAsync()
        {
            // stop
            //GET / cgi - bin / capture_notimeout ? iface = eth_udma0 & minor = 1 & type = 2 & capture = Stop & sid = c64fcae4916dbdd9 & useajax = 1 & xhr = 1 & t1646748233367 = nocache HTTP / 1.1

            // start
            //GET / cgi - bin / capture_notimeout ? sid = c64fcae4916dbdd9 & capture = Start & snaplen = 1600 & ifaceorminor = 2 - 1 HTTP / 1.1


            //Uri hostListPathUri = new Uri(FormattableString.Invariant($"https://{FritzServiceOperationHandler.InternetGatewayDevice!.PreferredLocation.Host}:{FritzServiceOperationHandler.InternetGatewayDevice.SecurityPort}{hostListPath}"));

            string sid = await GetSidAsync();
            //string snaplen = $"1600";
            string iface = "2-1";
            string capturePath = $"/cgi-bin/capture_notimeout"; //?ifaceorminor =";
            Uri captureUri = new Uri(FormattableString.Invariant($"https://{FritzServiceOperationHandler.InternetGatewayDevice!.PreferredLocation.Host}:{FritzServiceOperationHandler.InternetGatewayDevice.SecurityPort}{capturePath}&sid={sid}{iface}&snaplen=1600&capure=Start"));


            //var theResult = await captureControlService.GetCaptureMemberAsync(captureUri);
            await Task.Delay(1000);

        }


        //IEnumerable<DeviceHost> deviceHosts = await deviceHostsService.GetDeviceHostsAsync(hostListPathUri);

        //private async Task GetCaptureControlMemberAsync()
        //{
            // Uri hostListPathUri = new Uri(FormattableString.Invariant($"https://{FritzServiceOperationHandler.InternetGatewayDevice!.PreferredLocation.Host}:{FritzServiceOperationHandler.InternetGatewayDevice.SecurityPort}{hostListPath}"));
            // $FRITZIP / cgi - bin / capture_notimeout ? ifaceorminor =$IFACE\&snaplen =\&capture = Start\&sid =$SID | ntopng - i -
            //return HttpGetRaw(captureUrl + "?sid=" + SessionID + "&ifaceorminor=" + iface + "&capture=Start&snaplen=" + snaplen);

            //http://fritz.box/cgi-bin/capture_notimeout?sid=0000000000000&capture=Start&snaplem=1600&ifaceorminor=2-1



            //string sid = FritzServiceOperationHandler.InternetGatewayDevice.
            //string snaplen = $"";
            //string iface = "1";
            //string capturePath = $"/cgi-bin/capture_notimeout"; //?ifaceorminor =";
            //Uri captureUri = new Uri(FormattableString.Invariant($"https://{FritzServiceOperationHandler.InternetGatewayDevice!.PreferredLocation.Host}:{FritzServiceOperationHandler.InternetGatewayDevice.SecurityPort}{capturePath}&sid={sid}{iface}\&snaplen=1600&capure=Start\&sid=$SID"));




            //Uri testUri = new Uri("hallo");

            //var theResult = captureControlService.GetDeviceHostsAsync(testUri);

            //captureControl = await captureControlService.GetCaptureMemberAsync(testUri);


            //HostsGetHostNumberOfEntriesResponse = await FritzServiceOperationHandler.GetHostsGetHostNumberOfEntriesAsync();
       // }



        
        protected override void FritzServiceViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.FritzServiceViewModelPropertyChanged(sender, e);
            
            switch (e.PropertyName)
            {
                case nameof(ProvisioningCode):
                
                    {
                        UpdateCanExecuteDefaultCommand();
                        break;
                    }
            }

        }
        
    }
}