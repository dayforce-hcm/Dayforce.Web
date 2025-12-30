#if NET
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif
using Controller = Dayforce.Web.Adapter.Controller;
using TestModels;

namespace WebProcessorLibrary.Areas.TestArea.Controllers;

/// <summary>
/// Test controller within the TestArea to demonstrate AreaRegistration
/// </summary>
[Area("TestArea")]
public class AreaTestController : Controller
{
    /// <summary>
    /// GET /TestArea/AreaTest/Index
    /// Returns the area's main view with area-specific layout and styling
    /// </summary>
    [HttpGet]
    public ActionResult Index() => View();

    /// <summary>
    /// GET /TestArea/AreaTest/Hello
    /// Returns information about the current area (JSON endpoint for testing)
    /// </summary>
    [HttpGet]
    public JsonResult Hello() => Json(TestModel.New($"{RouteData.Values["controller"]}.{RouteData.Values["action"]}"), JsonRequestBehavior.AllowGet);
}
