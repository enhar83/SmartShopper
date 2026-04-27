using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.AdminLayout
{
    public class _AdminLayoutSwitcherComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
