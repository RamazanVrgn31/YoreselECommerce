using System;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace Core.Utilities.Mail
{
    public class MailManager : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            //message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            // "Kimden" listesi boşsa, appsettings.json'daki varsayılan göndericiyi kullan.
            if (emailMessage.FromAddresses == null || emailMessage.FromAddresses.Count == 0)
            {
                var defaultSenderName = _configuration.GetSection("EmailConfiguration").GetSection("SenderName").Value;
                var defaultSenderAddress = _configuration.GetSection("EmailConfiguration").GetSection("SenderEmail").Value;

                // Eğer appsettings'teki adres de boşsa (test@domain.com gibi), garanti olsun diye UserName'i kullan
                if (string.IsNullOrEmpty(defaultSenderAddress) || defaultSenderAddress.Contains("test@domain"))
                {
                    defaultSenderAddress = _configuration.GetSection("EmailConfiguration").GetSection("UserName").Value;
                }

                message.From.Add(new MailboxAddress(defaultSenderName, defaultSenderAddress));
            }
            else
            {
                // Doluysa mevcut listeyi ekle
                message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            }

            message.Subject = emailMessage.Subject;


            var messageBody =  emailMessage.Content;

            message.Body = new TextPart(TextFormat.Html)
            {
                Text = messageBody
            };
            using var emailClient = new SmtpClient();

            // ⭐ 1. SERTİFİKA HATALARINI GÖRMEZDEN GEL (Development ortamı için şart)
            // Ethereal gibi test sunucularının sertifikası genelde geçersizdir, bunu yapmazsak hata verir.
            emailClient.CheckCertificateRevocation = false;
            emailClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

            emailClient.Connect(
                _configuration.GetSection("EmailConfiguration").GetSection("SmtpServer").Value,
                Convert.ToInt32(_configuration.GetSection("EmailConfiguration").GetSection("SmtpPort").Value),
                SecureSocketOptions.Auto);

            // ⭐⭐⭐ 2. GİRİŞ YAP (BU SATIR EKSİKTİ!) ⭐⭐⭐
            // appsettings.json'dan Kullanıcı Adı ve Şifreyi alıp giriş yapıyoruz.

            emailClient.Authenticate(
                _configuration.GetSection("EmailConfiguration").GetSection("Username").Value,
                _configuration.GetSection("EmailConfiguration").GetSection("Password").Value);
            emailClient.Send(message);
            emailClient.Disconnect(true);
        }
    }
}