using System.Web.Mvc;

namespace WebProcessorLibrary.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public ActionResult Index() => View();

    [HttpGet]
    public ActionResult Ping() => new EmptyResult();
}
