using Microsoft.AspNet.Mvc;

namespace Genesis.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly GenesisContext context;

        public HomeController(GenesisContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            ViewBag.Context = context;
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
