using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    // Injetamos a configuração para ler as senhas escondidas
    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Pega o e-mail e a senha do arquivo de configuração (ou variável de ambiente no Render)
        var mailUser = _config["Smtp:User"];
        var mailPass = _config["Smtp:Pass"];

        if (string.IsNullOrEmpty(mailUser) || string.IsNullOrEmpty(mailPass))
        {
            throw new InvalidOperationException("SMTP credentials are not configured properly.");
        }

        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(mailUser, mailPass)
        };

        return client.SendMailAsync(
            new MailMessage(from: mailUser, to: email, subject, htmlMessage) { IsBodyHtml = true }
        );
    }
}