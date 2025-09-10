using Microsoft.AspNetCore.Mvc;

namespace MunicipalConnect.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}