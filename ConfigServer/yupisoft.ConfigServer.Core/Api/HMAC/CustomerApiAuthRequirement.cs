using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{

    public class CustomerApiAuthRequirement : IAuthorizationRequirement
    {
    }
    public class CheckCustomerUserAuthorizationHandler : AuthorizationHandler<ClusterApiAuthRequirement>
    {
        private readonly IHttpContextAccessor _accessor;
        public CheckCustomerUserAuthorizationHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ClusterApiAuthRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == "Customer"))
            {
                context.Succeed(requirement);
            }
        }
    }
}
