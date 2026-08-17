using System.Net;
using System.Net.Mail;

namespace AutorisationMVC.Services;

public class EmailSender:IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var mail = "aerfceje@gmail.com";
        var password = "jpcn uesg onft bfml";
        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, password),
        };
        return client.SendMailAsync(new MailMessage(from:mail,to:email,subject,message));
    }
}