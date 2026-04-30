using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Layout
{
    public class _LayoutHeadComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
