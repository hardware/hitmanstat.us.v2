using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hitmanstat.us.Models;

namespace hitmanstat.us.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("/events")]
        public IActionResult Events()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            return View(new ErrorViewModel {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorCode = code
            });
        }
    }
}
