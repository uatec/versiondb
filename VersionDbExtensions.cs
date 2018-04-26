using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace VersionDb
{
    public static class VersionDbExtensions
    {
        public static void VersionDb<TCurrentVersionType>(this IApplicationBuilder app, string typename, VersionMapper<TCurrentVersionType> versionMapper)
        {
            var routeBuilder = new RouteBuilder(app);

            string path = $"{typename}/{{version}}/{{id}}";

            routeBuilder.MapGet(path, context =>
            {
                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                // TODO: Not Found
                VersionedDocument versionedDocument = Database<VersionedDocument>.Get(id);
                
                object mappedOutput = versionMapper.ToVersion(versionedDocument, requestedVersion);

                return context.Response.WriteAsync(JsonConvert.SerializeObject(mappedOutput));
            });

            routeBuilder.MapPost(path, context => {

                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                string body = context.Request.Body.ReadAllText();

                VersionedDocument versionedDocument = versionMapper.Parse(body, requestedVersion);

                Database<VersionedDocument>.Put(id, versionedDocument);

                return context.Response.WriteAsync("ok");
            });

            // TODO: Delete operations
            // TODO: Correct Put/Post behaviour

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }        
    }
}
