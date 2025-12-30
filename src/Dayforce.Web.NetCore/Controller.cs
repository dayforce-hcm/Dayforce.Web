using Microsoft.AspNetCore.Mvc;

namespace Dayforce.Web.Adapter;

public class Controller : Microsoft.AspNetCore.Mvc.Controller
{
    public JsonResult Json(object data, JsonRequestBehavior _) => base.Json(data);
}
