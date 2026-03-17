namespace SmartRollCall.Api.Services
{
    public interface IFaceDetectionService
    {
        Task<bool> IsFacePresentAsync(string imageBase64);
    }
}