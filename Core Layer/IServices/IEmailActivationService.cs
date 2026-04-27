using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.IServices
{
    public interface IEmailActivationService
    {
        Task TSendConfirmEmailAsync(string receiverEmail, string code);
    }
}
