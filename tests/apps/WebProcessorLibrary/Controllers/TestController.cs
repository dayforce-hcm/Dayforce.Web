#if NET
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif
using Controller = Dayforce.Web.Adapter.Controller;
using TestModels;

namespace WebProcessorLibrary.Controllers;

public partial class TestController : Controller
{
    [HttpGet]
    public JsonResult Ping() => Json(TestModel.New("Get Ping"), JsonRequestBehavior.AllowGet);

    [HttpPost]
    public JsonResult Pong() => Json(TestModel.New("Post Pong"), JsonRequestBehavior.AllowGet);
}
