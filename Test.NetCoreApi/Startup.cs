using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.DJ.ImplementFactory;
using System.DJ.ImplementFactory.MServiceRoute;
using System.IO;
using System.Text.RegularExpressions;

namespace Test.NetCoreApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ImplementAdapter.Init();
            MService.Start();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc(options =>
            {
                //options.Filters.Add<FilterMvcController>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            string originFile = Path.Combine(Directory.GetCurrentDirectory(), "Origin.json");
            if (File.Exists(originFile))
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Origin.json", optional: true)
                .Build();

                string uris = config["Uris"];
                Regex rg = new Regex(@"[^a-z0-9_\s\:\/\-]", RegexOptions.IgnoreCase);
                char c = ',';
                if (rg.IsMatch(uris))
                {
                    string s = rg.Match(uris).Groups[0].Value;
                    c = s.ToCharArray()[0];
                }
                string[] arr = uris.Split(c);
                int size = arr.Length;
                for (int i = 0; i < size; i++)
                {
                    string key = arr[i];
                    key = key.Trim();
                    key = key.Replace("\\", "/");
                    if (key.Substring(key.Length - 1).Equals("/")) key = key.Substring(0, key.Length - 1);
                    arr[i] = key;
                }

                app.UseCors(builder =>
                {
                    builder.WithOrigins(arr)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            }

            //app.Use(UseFilterController.Filter());

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
