using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Settings
{
    public class MailSettings
    {
        public required string Server { get; set; }
        public int Port { get; set; }
        public required string SenderName { get; set; }
        public required string SenderEmail { get; set; }
        public required string Password { get; set; }
    }
}
