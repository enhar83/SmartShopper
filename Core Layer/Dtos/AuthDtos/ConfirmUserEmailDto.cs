using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_Layer.Dtos.AuthDtos
{
    public class ConfirmUserEmailDto
    {
        public required string Email { get; set; }
        public int ActivationCode { get; set; }
    }
}
