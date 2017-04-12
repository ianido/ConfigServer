using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Authentication.Hmac
{
    public class ValidateCredentialsContext : BaseControlContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="ValidateCredentialsContext"/>.
        /// </summary>
        /// <param name="context">The HttpContext the validate context applies too.</param>
        /// <param name="options">The <see cref="BasicAuthenticationOptions"/> for the instance of 
        /// <see cref="BasicAuthenticationMiddleware"/> is creating this instance.</param>
        public ValidateCredentialsContext(HttpContext context, HmacAuthenticationOptions options)
            : base(context)
        {
        }

    }
}
