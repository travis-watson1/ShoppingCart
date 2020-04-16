using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CmsShoppingCart
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                //options.IdleTimeout = TimeSpan.FromSeconds(2);
            });
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllersWithViews();

            //services.AddEntityFrameworkNpgsql().AddDbContext<CmsShoppingCartContext>(options =>
            //    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddEntityFrameworkNpgsql().AddDbContext<CmsShoppingCartContext>(options =>
            {
                var databaseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                var databaseUri = new Uri(databaseUrl);
                var userInfo = databaseUri.UserInfo.Split(':');

                string connStr;

                // Depending on if in development or production, use either Heroku-provided
                // connection string, or development connection string from env var.
                if (databaseUrl == "Development")
                {
                    // Use connection string from file.
                    connStr = Configuration.GetConnectionString("DefaultConnection");
                }
                else
                {
                    string _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
                    _connectionString.Replace("//", "");

                    char[] delimiterChars = { '/', ':', '@', '?' };
                    string[] strConn = _connectionString.Split(delimiterChars);
                    strConn = strConn.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    var pgUser = strConn[1];
                    var pgPassword = strConn[2];
                    var pgServer = strConn[3];
                    var pgDatabase = strConn[5];
                    var pgPort = strConn[4];
                    connStr =
                        $"User ID={pgUser};Password={pgPassword};Host={pgServer};Port={pgPort};Database={pgDatabase};Pooling=true;Use SSL Stream=True;SSL Mode=Require;TrustServerCertificate=True;";
                    //connStr = "host=" + pgServer + ";port=" + pgPort + ";database=" + pgDatabase + ";uid=" + pgUser + ";pwd=" + pgPassword + ";sslmode=Require;Trust Server Certificate=true;Timeout=1000";
                }

                // Whether the connection string came from the local development configuration file
                // or from the environment variable from Heroku, use it to set up your DbContext.
                options.UseNpgsql(connStr);
            });




            //services.AddDbContext<CmsShoppingCartContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CmsShoppingCartContext")));
            services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<CmsShoppingCartContext>()
                .AddDefaultTokenProviders();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "pages",
                    "{slug?}",
                    defaults: new {controller = "Pages", action = "Page" });

                endpoints.MapControllerRoute(
                    "products",
                    "products/{categorySlug}",
                    defaults: new { controller = "Products", action = "ProductsByCategory" });

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
