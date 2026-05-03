using System.Security.Claims;
using Bogus.DataSets;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class UserAddressController : Controller
    {
        private readonly IUserAddressService _userAddressService;

        public UserAddressController(IUserAddressService userAddressService)
        {
            _userAddressService = userAddressService;
        }

        public async Task<IActionResult> UserAddressList()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdString);

            var userAddresses = await _userAddressService.TGetUserAddressListAsync(userId);
            return Json(new { succeeded = true, data = userAddresses });
        }
    }
}
