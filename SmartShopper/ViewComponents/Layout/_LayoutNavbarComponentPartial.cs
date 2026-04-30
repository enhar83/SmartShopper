using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Layout
{
    public class _LayoutNavbarComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
