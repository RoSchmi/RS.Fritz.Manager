namespace RS.Fritz.Manager.API
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface ICaptureControlService
    {
        Task<bool> GetStartCaptureResponseSocketAsync(string scheme, string host, string capturePath, string query, int captureFileSizeMB);

        Task<bool> GetStartCaptureResponseStreamAsync(Uri uri, string folderPath, string filePrefix);

        Task GetStartCaptureResponseAsync(Uri uri, string folderPath, string filePrefix);

        Task GetStopCaptureResponseAsync(Uri uri);
    }
}