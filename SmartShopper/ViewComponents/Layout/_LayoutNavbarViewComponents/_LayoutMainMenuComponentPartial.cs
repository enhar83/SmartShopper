using Microsoft.AspNetCore.Mvc;

namespace SmartShopper.ViewComponents.Layout._LayoutNavbarViewComponents
{
    public class _LayoutMainMenuComponentPartial:ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
