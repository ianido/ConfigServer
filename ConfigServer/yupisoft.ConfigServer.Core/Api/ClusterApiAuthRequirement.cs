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

    public class ClusterApiAuthRequirement : IAuthorizationRequirement
    {
    }
    public class CheckClusterUserAuthorizationHandler : AuthorizationHandler<ClusterApiAuthRequirement>
    {
        private readonly IHttpContextAccessor _accessor;
        public CheckClusterUserAuthorizationHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ClusterApiAuthRequirement requirement)
        {
            if (context.User.HasClaim(claim => claim.Type == ClaimTypes.Role && claim.Value == "Cluster"))
            {
                await Task.Run(() => context.Succeed(requirement));                
            }
        }
    }
}
