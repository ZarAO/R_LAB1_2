using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shed.CoreKit.WebApi;

namespace MicroCommerce.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                //  when root calls, the start page will be returned
                if (string.IsNullOrEmpty(context.Request.Path.Value.Trim('/')))
                {
                    context.Request.Path = "/index.html";
                }

                await next();
            });
            app.UseStaticFiles();
            // редиректы на микросервисы
            app.UseWebApiRedirect("api/products", new WebApiEndpoint<IProductCatalog>(new System.Uri("http://localhost:5001")));
            app.UseWebApiRedirect("api/orders", new WebApiEndpoint<IShoppingCart>(new System.Uri("http://localhost:5002")));
            app.UseWebApiRedirect("api/logs", new WebApiEndpoint<IActivityLogger>(new System.Uri("http://localhost:5003")));
        }
    }
}