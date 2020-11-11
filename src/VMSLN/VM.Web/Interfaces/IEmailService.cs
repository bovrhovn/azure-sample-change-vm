using System.Threading.Tasks;

namespace VM.Web.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string from, string to,string subject, string body);
    }
}