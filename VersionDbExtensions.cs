using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace VersionDb
{
    public static class VersionDbExtensions
    {
        public static void VersionDb(this IApplicationBuilder app, string typename, List<KeyValuePair<string, Type>> versions)
        {
            var routeBuilder = new RouteBuilder(app);

            string path = $"{typename}/{{version}}/{{id}}";

            routeBuilder.MapGet(path, context =>
            {
                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                // TODO: Not Found
                VersionedDocument versionedDocument = Database<VersionedDocument>.Get(id);
                
                // TODO: Handle unknown versions
                Type requestedType = versions.Single(p => p.Key == requestedVersion).Value;
                Type dataType = versions.Single(p => p.Key == versionedDocument.Version).Value;

                // TODO: do some iterative mapping up or down the version chain until we get the one we want.
                object mappedOutput = AutoMapper.Mapper.Map(versionedDocument.Document, dataType, requestedType);
                
                return context.Response.WriteAsync(JsonConvert.SerializeObject(mappedOutput));
            });

            routeBuilder.MapPost(path, context => {

                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                // TODO: Handle unknown versions
                Type type = versions.Single(p => p.Key == requestedVersion).Value;

                string body = null;
                using ( var reader = new StreamReader(context.Request.Body))
                {
                    body = reader.ReadToEnd();
                }

                object data = JsonConvert.DeserializeObject(body, type);

                Database<VersionedDocument>.Put(id, new VersionedDocument { Version = requestedVersion, Document = data });

                return context.Response.WriteAsync("ok");
            });

            // TODO: Delete operations
            // TODO: Correct Put/Post behaviour

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }        
    }
}
