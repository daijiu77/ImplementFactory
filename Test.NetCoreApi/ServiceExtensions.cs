using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.DJ.ImplementFactory.Commons;
using System.IO;
using System.Text.RegularExpressions;

namespace Test.NetCoreApi
{
    public static class ServiceExtensions
    {
        private static string myUrl = "https://blog.csdn.net/u010465417/article/details/105603908";
        public static string ApiName = "";
        public static string AssemblyName = "";

        public static void SetAssemblyName(string assemblyName)
        {
            string api_name = assemblyName;
            int n = api_name.LastIndexOf(".");
            ApiName = api_name.Substring(0, n);
            AssemblyName = assemblyName;
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V1", new OpenApiInfo
                {
                    // {ApiName} 定义成全局变量，方便修改
                    Version = "V1",
                    Title = $"{ApiName} 接口文档——Netcore",
                    Description = $"{ApiName} HTTP API V1",
                    Contact = new OpenApiContact { Name = ApiName, Email = "Blog.Core@xxx.com", Url = new Uri(myUrl) },
                    License = new OpenApiLicense { Name = ApiName, Url = new Uri(myUrl) }
                });
                c.OrderActionsBy(o => o.RelativePath);

                var fileName = AssemblyName + ".xml";
                string XmlCommentsFilePath = Path.Combine(DJTools.RootPath, fileName);
                c.IncludeXmlComments(XmlCommentsFilePath, true);//默认的第二个参数是false，这个是controller的注释，记得修改
            });
        }

        public static void AddSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"NetCore V1");
                //c.RoutePrefix = string.Empty;     //如果是为空 访问路径就为 根域名/index.html,注意localhost:8001/swagger是访问不到的
                //路径配置，设置为空，表示直接在根域名（localhost:8001）访问该文件
                c.RoutePrefix = "swagger"; // 如果你想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "swagger"; 则访问路径为 根域名/swagger/index.html
            });
        }

        public static void AddOrigin(this IApplicationBuilder app, string originFileName)
        {
            string originFile = Path.Combine(DJTools.RootPath, originFileName);
            if (File.Exists(originFile))
            {
                IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(originFileName, optional: true)
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
        }
    }
}
