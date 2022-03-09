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

        public async Task<bool> GetStartCaptureResponseAsync(Uri uri)
        {
            HttpClient httpClient = httpClientFactory.CreateClient(Constants.HttpClientName);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var cancToken = cancellationTokenSource.Token;

            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePrefix = "FritzboxCapture";

            var file = new FileInfo(FormattableString.Invariant($"{folderPath}\\{filePrefix}_{DateTime.Now.ToString("dd/MM/yyyy' 'HH' Uhr 'mm'-'ss")}.eth"));

           // Alternative 2:
           //var downloadStream = httpClient.GetStreamAsync(uri, cancToken).Result;

            // Alternative 1:
            var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using (var downloadStream = await response.Content.ReadAsStreamAsync(cancToken))
            {
                using (var fileStream = file.Create())
                {
                    downloadStream.CopyTo(fileStream);
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