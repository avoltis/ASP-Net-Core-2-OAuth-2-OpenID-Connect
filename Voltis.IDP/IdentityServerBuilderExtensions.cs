using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voltis.IDP.Services;

namespace Voltis.IDP
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddVoltisUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddScoped<IVoltisUserRepository, VoltisUserRepository>();
            builder.AddProfileService<VoltisUserProfileService>();

            return builder;
        }
    }
}
