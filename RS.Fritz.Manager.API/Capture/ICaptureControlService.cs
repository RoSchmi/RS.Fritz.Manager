namespace RS.Fritz.Manager.API
{
    using System;
    using System.Threading.Tasks;

    public interface ICaptureControlService
    {
        Task<bool> GetStartCaptureResponseAsync(Uri uri);

        Task<bool> GetStopCaptureResponseAsync(Uri uri);
    }
}