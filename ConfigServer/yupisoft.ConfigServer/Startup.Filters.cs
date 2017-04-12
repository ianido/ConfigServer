namespace yupisoft.ConfigServer
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Authorization;

    public partial class Startup
    {
        /// <summary>
        /// Adds filters which help improve security.
        /// </summary>
        /// <param name="environment">The environment the application is running under. This can be Development,
        /// Staging or Production by default.</param>
        private static void ConfigureFilters(IHostingEnvironment environment, FilterCollection filters)
        {

        }
    }
}
