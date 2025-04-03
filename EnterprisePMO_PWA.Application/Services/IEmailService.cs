using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Interface for email service operations
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email to the specified recipient
        /// </summary>
        /// <param name="to">Email address of the recipient</param>
        /// <param name="subject">Subject of the email</param>
        /// <param name="body">HTML body of the email</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendEmailAsync(string to, string subject, string body);
    }
} 