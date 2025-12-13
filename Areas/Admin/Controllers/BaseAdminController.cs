using System.Web.Mvc;
using System.Web.Routing;

namespace WebAppHealth.Areas.Admin.Controllers
{
    [Authorize]
    public class BaseAdminController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionRole = Session["Role"] as string;

            if (string.IsNullOrEmpty(sessionRole) ||
               (sessionRole != "Admin" && sessionRole != "Staff" && sessionRole != "Doctor"))
            {
                // Hủy request hiện tại, chuyển hướng về Login
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login", area = "" })
                );
            }

            base.OnActionExecuting(filterContext);
        }

        protected void SetAlert(string message, string type)
        {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = type;
        }
    }
}