using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace webCore.Controllers
{
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool isAnonymousAccessAllowed = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

            if (!isAnonymousAccessAllowed)
            {
                var sessionUid = context.HttpContext.Session.GetString("uid");
                if (string.IsNullOrEmpty(sessionUid))
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
