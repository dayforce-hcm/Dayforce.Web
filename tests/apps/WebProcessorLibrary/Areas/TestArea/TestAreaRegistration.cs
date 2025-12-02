using System.Web.Mvc;

namespace WebProcessorLibrary.Areas.TestArea;

/// <summary>
/// Demonstrates System.Web.Mvc.AreaRegistration
/// Custom area registration for organizing related controllers
/// </summary>
public class TestAreaRegistration : AreaRegistration
{
    public override string AreaName => "TestArea";

    public override void RegisterArea(AreaRegistrationContext context)
    {
        context.MapRoute(
            "TestArea_default",
            "TestArea/{controller}/{action}/{id}",
            new { action = "Index", id = UrlParameter.Optional }
        );
    }
}
