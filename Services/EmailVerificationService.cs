using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace JewelryShop.Services
{
    public class EmailVerificationService
    {
        private readonly string _fromEmail = "SonLVHE172736@fpt.edu.vn";
        private readonly string _appPassword = "tkcf mtsi knfe ciew";
        private readonly string _frontendUrl = "http://localhost:5173";

        public string GenerateVerificationToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "");
            }
        }

        public void SendVerificationEmail(string email, string token)
        {
            string verifyUrl = $"{_frontendUrl}/verify-email?token={token}";

            string subject = "Xác thực tài khoản Jewelry Shop";
            string body = $@"
                <h2>Chào bạn,</h2>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại Jewelry Shop.</p>
                <p>Vui lòng nhấn vào liên kết bên dưới để xác thực email của bạn:</p>
                <a href='{verifyUrl}'>Xác thực tài khoản</a>
                <br/><br/>
                <p>Liên kết này sẽ hết hạn sau 15 phút.</p>
            ";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_fromEmail, "Jewelry Shop");
                message.To.Add(email);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                    client.Send(message);
                }
            }
        }
        public void SendEmail(string email, string subject, string body)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(_fromEmail, "Jewelry Shop");
                message.To.Add(email);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_fromEmail, _appPassword);
                    client.Send(message);
                }
            }
        }


    }
}
