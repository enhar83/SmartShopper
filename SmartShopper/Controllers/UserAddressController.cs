using System.Security.Claims;
using Bogus.DataSets;
using Core_Layer.Dtos.UserAddressDtos;
using Core_Layer.Exceptions;
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

        public async Task<IActionResult> GetUserAddresses()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
                return Unauthorized();

            var userId = Guid.Parse(userIdString);

            var userAddresses = await _userAddressService.TGetUserAddressListAsync(userId);
            return Json(new { succeeded = true, data = userAddresses });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(AddUserAddressDto addUserAddressDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new { succeeded = false, errors = errors });
            }

            try
            {
                await _userAddressService.TAddUserAddressAsync(addUserAddressDto);

                return Ok(new { succeeded = true, message = "Address has been successfully added." });
            }
            catch (LogicException ex)
            {
                return BadRequest(new { succeeded = false, errors = new[] { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { succeeded = false, errors = new[] { "A technical error occurred while saving the address." } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> UpdateAddress(UpdateUserAddressDto updateUserAddressDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new { succeeded = false, errors = errors });
            }

            try
            {
                await _userAddressService.TUpdateUserAddressAsync(updateUserAddressDto);
                return Ok(new { succeeded = true, message = "Address has been updated successfully." });
            }
            catch (LogicException ex)
            {
                return BadRequest(new { succeeded = false, errors = new[] { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new { succeeded = false, errors = new[] { "An unexpected technical error occurred while updating the address." } });
            }
        }
    }
}
