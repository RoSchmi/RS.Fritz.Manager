namespace RS.Fritz.Manager.API
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class CaptureControlService : ICaptureControlService
    {
        private readonly IHttpClientFactory httpClientFactory;

        private const int BufferSize = 1024;
        private byte[] buffer = new byte[BufferSize];

        public CaptureControlService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool> GetStartCaptureResponseSocketAsync(string scheme, string host, string capturePath, string query)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localIpAddress;
            IPAddress remoteIpAddress = IPAddress.Parse("192.168.1.1");

            if (ipHostInfo.AddressList[0].AddressFamily == remoteIpAddress.AddressFamily)
            {
                localIpAddress = ipHostInfo.AddressList[0];
            }
            else
            {
                localIpAddress = ipHostInfo.AddressList[1];
            }

            const int fritzPort = 80;
            const string CrLf = "\r\n";
            const string Version = " HTTP/1.1";
            const string Enc = " gzip, deflate, br";
            string request = FormattableString.Invariant($"GET {capturePath}?{query}{Version}{CrLf}Host: {host}{CrLf}Accept-Encoding:{Enc}{CrLf}{CrLf}");

            IPEndPoint localEndPoint = new IPEndPoint(localIpAddress, fritzPort);
            IPEndPoint fritzEndPoint = new IPEndPoint(remoteIpAddress, fritzPort);

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            const string filePrefix = "FritzboxCapture";
            const string responseFilePrefix = "FritzboxCaptureResponse";

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancToken = cancellationTokenSource.Token;

            var responsefile = new FileInfo(FormattableString.Invariant($"{folderPath}\\{responseFilePrefix}_{DateTime.Now.ToString("dd/MM/yyyy'_'HH'_'mm'.'ss")}.txt"));
            var responseFileStream = responsefile.Create();

            var file = new FileInfo(FormattableString.Invariant($"{folderPath}\\{filePrefix}_{DateTime.Now.ToString("dd/MM/yyyy'_'HH'_'mm'.'ss")}.eth"));
            var fileStream = file.Create();

            Socket clientSocket = new Socket(localIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await clientSocket.ConnectAsync(fritzEndPoint);
            await clientSocket.SendAsync(Encoding.UTF8.GetBytes(request), SocketFlags.None);

            const int buffersize = 4096;
            byte[] receiveBuffer = new byte[buffersize];
            bool isFinished = false;
            int rounds = 0;
            int actIndex = 0;

            // 400 represents the limit for the file size 400 x 4096 = about 1.6 MBytes
            while (rounds < 4000 && !isFinished)
            {
                await Task.Delay(2);
                int bytesRead = await clientSocket.ReceiveAsync(receiveBuffer, SocketFlags.None, cancToken);
                isFinished = bytesRead < buffersize;
                if (rounds == 0)
                {
                    // Find end of request headers to differently handle response and payload
                    while ((receiveBuffer[actIndex + 1] != 10) || (receiveBuffer[actIndex + 2] != 13) || receiveBuffer[actIndex + 3] != 10)
                    {
                        actIndex = Array.IndexOf(receiveBuffer, (byte)13, actIndex + 1);
                    }

                    //string beginString = Encoding.UTF8.GetString(receiveBuffer, 0, actIndex + 3);

                    await responseFileStream.WriteAsync(receiveBuffer, 0, actIndex + 3);
                    await fileStream.WriteAsync(receiveBuffer, actIndex + 4, bytesRead - (actIndex + 4), cancToken);
                    responseFileStream.Close();
                }
                else
                {
                    await fileStream.WriteAsync(receiveBuffer, 0, bytesRead, cancToken);
                }

                rounds++;
            }

            fileStream.Close();
            return true;
        }

        public async Task<bool> GetStartCaptureResponseStreamAsync(Uri uri)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancToken = cancellationTokenSource.Token;
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePrefix = "FritzboxCapture";
            var file = new FileInfo(FormattableString.Invariant($"{folderPath}\\{filePrefix}_{DateTime.Now.ToString("dd/MM/yyyy' 'HH'_'mm'.'ss")}.eth"));
            var fileStream = file.Create();

            byte[] buffer = new byte[4096];
            int loops = 0;
            int allBytesRead = 0;
            int bytesRead = -1;
            
            var responseStream = await httpClient.GetStreamAsync(uri,cancToken);
            while (!cancToken.IsCancellationRequested)
            {
                try
                {
                    bytesRead = await responseStream.ReadAsync(buffer, cancToken);
                }
                catch (Exception ex)
                {
                    string message1 = ex.Message;
                }

                try
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancToken);
                }
                catch (Exception ex)
                {
                    string message2 = ex.Message;
                }

                allBytesRead += bytesRead;
                loops++;
                if (loops == 3)
                {
                    cancellationTokenSource.Cancel();
                    await Task.Delay(1);
                }
            }

            fileStream.Close();



            


            /*
            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);


            response.EnsureSuccessStatusCode();
            using (var downloadStream = await response.Content.ReadAsStreamAsync(cancToken))
            {
                using (var fileStream = file.Create())
                {
                    await downloadStream.CopyToAsync(fileStream);
                }
            }
            */

            return true;
        }

        public async Task<bool> GetStartCaptureResponseAsync(Uri uri)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancToken = cancellationTokenSource.Token;
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePrefix = "FritzboxCapture";

            var file = new FileInfo(FormattableString.Invariant($"{folderPath}\\{filePrefix}_{DateTime.Now.ToString("dd/MM/yyyy'_'HH'_'mm'.'ss")}.eth"));

            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using (var downloadStream = await response.Content.ReadAsStreamAsync(cancToken))
            {
                using (var fileStream = file.Create())
                {
                    await downloadStream.CopyToAsync(fileStream);
                }
            }

            return true;
        }

        public async Task<bool> GetStopCaptureResponseAsync(Uri uri)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}