namespace RS.Fritz.Manager.API
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class CaptureControlService : ICaptureControlService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CaptureControlService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetStartCaptureResponseAsync(Uri uri)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancToken = cancellationTokenSource.Token;

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.my);
            string filePrefix = "FritzboxCapture";

            var file = new FileInfo(FormattableString.Invariant($"{folderPath}\\{filePrefix}_{DateTime.Now.ToString("dd/MM/yyyy' 'HH' Uhr 'mm'-'ss")}.eth"));

            // Example 1:
            /*
            var downloadStream = httpClient.GetStreamAsync(uri, cancToken).Result;
            */

            // Example 2:
            
            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using (var downloadStream = await response.Content.ReadAsStreamAsync(cancToken))
            {
                using (var fileStream = file.Create())
                {
                    downloadStream.CopyTo(fileStream);
                }
            }

            int dummy34 = 1;

            //var downloadStream = await httpClient.GetAsync(uri, cancToken);

            // = httpClient.GetStreamAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            

            

            /*
            // Example for tests
            var file = new FileInfo(folderPath + @"\FritzboxCapture123.eth");
            StreamWriter str = file.CreateText();
            str.WriteLine("hello");
            Console.WriteLine("File has been created with text");
            str.Close();
            */


            /*
            var file = new FileInfo(folderPath + @"\FritzboxCapture123.eth");

            using (downloadStream)
            {
                using (var fileStream = file.Create())
                {
                    downloadStream.CopyTo(fileStream);
                }
            }
            */

            //Task<Stream> captureStream = httpClient.GetStreamAsync(uri);

            // Stream captureStream = httpClient.GetStreamAsync(uri);

            //string response = await httpClientFactory.CreateClient(Constants.HttpClientName).GetStringAsync(uri);
            //return response;
            return "Hallo";
        }

        public async Task<string> GetStopCaptureResponseAsync(Uri uri)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return "Hallo";
        }
    }
}