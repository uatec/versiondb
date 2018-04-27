using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace VersionDb.Demo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            // TODO: Choice of mapper
            AutoMapper.Mapper.Initialize(cfg => {
                cfg.CreateMap<V1.Order, V2.Order>()
                    .ForMember(c => c.Payments, opt => opt.MapFrom(src => MapPaymentInfo(src)));
                cfg.CreateMap<V2.Order, V1.Order>()
                    .ForMember(c => c.PaymentTotal, opt => opt.MapFrom(src => src.Payments.Amount))
                    .ForMember(c => c.PaymentDetails, opt => opt.MapFrom(src => MapPaymentDetails(src)));
            });

            services.AddSingleton<IDatabase<VersionedDocument>, Database<VersionedDocument>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO: More use of generics in extension method?
            app.VersionDb("order",
                new VersionMapper<V2.Order> (
                    "v2", 
                    new List<KeyValuePair<string, Type>> {
                        KeyValuePair.Create("v1", typeof(V1.Order)),
                        KeyValuePair.Create("v2", typeof(V2.Order))
                    }
                )
            );
        }

        private static V1.PaymentDetail[] MapPaymentDetails(V2.Order src)
        {
            return new [] {
                new V1.PaymentDetail {
                    Method = src.Payments.Method,
                    Date = src.Payments.Date
                }
            };
        }
        private static V2.PaymentInfo MapPaymentInfo(V1.Order src) 
        {
            var pd = src.PaymentDetails.Single();

            return new V2.PaymentInfo {
                Method = pd.Method,
                Date = pd.Date,
                Amount = src.PaymentTotal
            };
        }
    }
}
