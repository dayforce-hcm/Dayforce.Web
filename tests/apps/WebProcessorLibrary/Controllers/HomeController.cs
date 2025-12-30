#if NET
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif

namespace WebProcessorLibrary.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public ActionResult Index() => View();

    [HttpGet]
    public ActionResult Ping() => new EmptyResult();
}
