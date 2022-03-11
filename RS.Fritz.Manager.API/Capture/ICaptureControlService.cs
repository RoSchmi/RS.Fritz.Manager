namespace RS.Fritz.Manager.API
{
    using System;
    using System.Threading.Tasks;

    public interface ICaptureControlService
    {
        Task<bool> GetStartCaptureResponseSocketAsync(string scheme, string host, string capturePath, string query);

        Task<bool> GetStartCaptureResponseStreamAsync(Uri uri);

        Task<bool> GetStartCaptureResponseAsync(Uri uri);

        Task<bool> GetStopCaptureResponseAsync(Uri uri);
    }
}