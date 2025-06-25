using AIDMS.Shared.Constants.Permission;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace AIDMS.Server.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
            //var httpContext = context.GetHttpContext();

            //if(httpContext.User.Identity.IsAuthenticated)
            //    if(httpContext.User.IsInRole(Permissions.Hangfire.View))
            //        return true;

            //return false;
        }
    }
}
