using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SenseNet.Identity
{
    public static class IdentityExtensions
    {
        public static IdentityBuilder AddSenseNetIdentity(this IServiceCollection services)
        {
            //UNDONE: read configuration and provide values to instances
            //- or pass on a whole IConfiguration object
            //- or let them use the SnConfig API
            services.AddSingleton<IUserStore<SnIdentityUser>>(new SnUserStore());
            services.AddSingleton<IRoleStore<SnIdentityRole>>(new SnRoleStore());

            //These thingz are configured by AddIdentity below.
            //services.AddScoped<SignInManager<SnIdentityUser>, SignInManager<SnIdentityUser>>();
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services.AddIdentity<SnIdentityUser, SnIdentityRole>()
                .AddDefaultTokenProviders();
        }
    }
}
