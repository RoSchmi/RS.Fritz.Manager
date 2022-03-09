namespace RS.Fritz.Manager.API
{
    using System;
    using System.Threading.Tasks;

    public interface ICaptureControlService
    {
        Task<string> GetStartCaptureResponseAsync(Uri uri);

        Task<string> GetStopCaptureResponseAsync(Uri uri);
    }
}