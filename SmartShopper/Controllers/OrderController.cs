using System.Security.Claims;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Index", "Home");

            var userIdString = Guid.Parse(userId);

            var orders = await _orderService.TGetOrdersByUserIdAsync(userIdString);

            return View(orders);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var order = await _orderService.TGetOrderDetailsAsync(id);

                if (order == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred on the server side." });
            }
        }
    }
}
