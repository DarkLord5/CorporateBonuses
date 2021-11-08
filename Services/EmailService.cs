using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace CorporateBonuses.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Система запросов", "corpbonus@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", "corporatebonuses@yandex.ru"));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);//"smtp.yandex.ru", 25, true);
                await client.AuthenticateAsync("corpbonus@gmail.com", "123Cb456");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
