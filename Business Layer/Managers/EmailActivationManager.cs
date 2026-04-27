using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.IServices;
using Core_Layer.Settings;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace Business_Layer.Managers
{
    public class EmailActivationManager:IEmailActivationService
    {
        private readonly MailSettings _mailSettings;

        public EmailActivationManager(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task TSendConfirmEmailAsync(string receiverEmail, string code)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
            mimeMessage.To.Add(new MailboxAddress("Dear User", receiverEmail));
            mimeMessage.Subject = "SmartShopper | Email Verification";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
            <div style=""background-color: #f4f4f7; padding: 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
                    <tr>
                        <td style=""padding: 40px 20px; text-align: center; background-color: #2c3e50;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">SmartShopper</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h2 style=""color: #333333; font-size: 22px; margin-top: 0;"">Verify Your Email Address</h2>
                            <p style=""color: #555555; font-size: 16px; line-height: 1.5;"">
                                Welcome to the SmartShopper family! To complete your registration, please use the 6-digit verification code below:
                            </p>
                            <div style=""text-align: center; margin: 30px 0;"">
                                <span style=""display: inline-block; padding: 15px 30px; background-color: #f8f9fa; border: 2px dashed #2c3e50; border-radius: 5px; font-size: 32px; font-weight: bold; color: #2c3e50; letter-spacing: 5px;"">
                                    {code}
                                </span>
                            </div>
                            <p style=""color: #777777; font-size: 14px; line-height: 1.5;"">
                                This code is valid for 15 minutes. If you did not create this account, you can safely ignore this email.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 20px; background-color: #f8f9fa; text-align: center; border-top: 1px solid #eeeeee;"">
                            <p style=""color: #999999; font-size: 12px; margin: 0;"">
                                &copy; 2026 SmartShopper. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </div>";

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_mailSettings.Server, _mailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_mailSettings.SenderEmail, _mailSettings.Password);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
