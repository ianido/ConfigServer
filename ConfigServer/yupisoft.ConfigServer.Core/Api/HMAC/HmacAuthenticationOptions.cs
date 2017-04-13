using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Hmac;

namespace Microsoft.AspNetCore.Builder
{
    public class HmacAuthenticationOptions : AuthenticationOptions
    {
        public ulong MaxRequestAgeInSeconds { get; set; }
        public string AppId { get; set; }
        public string SecretKey { get; set; }
        public bool Encrypted { get; set; }

        public HmacAuthenticationOptions()
        {
            MaxRequestAgeInSeconds = HmacAuthenticationDefaults.MaxRequestAgeInSeconds;
            AuthenticationScheme = HmacAuthenticationDefaults.AuthenticationScheme;
            AutomaticChallenge = true;    // Esto es necesario para que el middleware procese el authentication sin especificarle
                                          // Authorize(AuthenticationScheme="Hmac"), si lo pongo en false obtengo un error 500 en ves de 401
            AutomaticAuthenticate = true; // Esto si no lo pongo en true el handle no guarda el identity, a menos que 
                                          // se le especifique en el Authorize(AuthenticationScheme="Hmac"), si lo pongo en false obtengo un error 403 en ves de 200
        }
    }
}
