using AIDMS.Shared.Application.Requests.Mail;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IMailService
    {
        Task SendAsync(MailRequest request);
    }
}
