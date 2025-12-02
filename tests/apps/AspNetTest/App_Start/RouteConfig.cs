using System.Web.Mvc;
using System.Web.Routing;
using WebProcessorLibrary.Controllers;

namespace AspNetTest.App_Start;

public static class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new
            {
                controller = nameof(HomeController)[..^"Controller".Length],
                action = nameof(HomeController.Index),
                id = UrlParameter.Optional
            }
        );
    }
}
