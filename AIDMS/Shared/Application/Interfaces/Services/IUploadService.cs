using AIDMS.Shared.Application.Requests;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IUploadService
    {
        string UploadAsync(UploadRequest request);
    }
}
