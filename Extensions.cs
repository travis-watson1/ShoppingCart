using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CmsShoppingCart.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CmsShoppingCart
{
    public static class Extensions
    {
        public static IWebHost MigrateDatabase(this IWebHost webHost)
        {
            // Manually run any pending migrations if configured to do so.
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (env == "Production")
            {
                var serviceScopeFactory = (IServiceScopeFactory)webHost.Services.GetService(typeof(IServiceScopeFactory));

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var dbContext = services.GetRequiredService<CmsShoppingCartContext>();

                    dbContext.Database.Migrate();
                }
            }

            return webHost;
        }
    }
}
