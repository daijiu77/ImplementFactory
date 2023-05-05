using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.MServiceRoute;
using System.DJ.ImplementFactory.MServiceRoute.Attrs;
using System.DJ.ImplementFactory.MServiceRoute.ServiceManager;
using Web.NetCore.Controllers;
using Web.NetCore.Models;

namespace Web.NetCore
{
    public class Startup
    {
        public static ServiceIPCollector serviceIPCollector = new ServiceIPCollector();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ImplementAdapter.Start();
            MService.Start();
            serviceIPCollector.Monitor(new CallMethodInfo()
            {
                ControllerType = typeof(HomeController),
                MethodName = "ReceiveManage"
            });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMvc(options =>
            {
                //options.Filters.Add<MSFilter>();
                options.Filters.Add<UIResultModel>();
                options.ModelBinderProviders.Insert(0, new ModelBinderProvider());
            });
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
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
