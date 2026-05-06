using Core_Layer.Dtos.OrderDtos;
using Core_Layer.IServices;
using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OrderList()
        {
            var orders = await _orderService.TGetOrdersForAdminAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] OrderStatusUpdateDto updateDto)
        {
            if (updateDto == null || updateDto.OrderId == Guid.Empty)
                return Json(new { success = false, message = "Invalid request data." });

            var result = await _orderService.TUpdateOrderStatusAsync(updateDto);

            if (result)
                return Json(new { success = true, message = "Order status updated successfully." });

            return Json(new { success = false, message = "An error occurred while updating the status." });
        }
    }
}
