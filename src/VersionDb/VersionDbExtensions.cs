using System;
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
            IDatabase<VersionedDocument> database = (IDatabase<VersionedDocument>) app.ApplicationServices.GetService(typeof(IDatabase<VersionedDocument>));
            
            var routeBuilder = new RouteBuilder(app);

            string path = $"{typename}/{{version}}/{{id}}";

            routeBuilder.MapGet(path, async context =>
            {
                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                if (context.Request.Query["watch"].Any())
                {
                    context.Response.ContentType = "text/event-stream";

                    foreach ( Change<VersionedDocument> change in database.Watch(id) )
                    {
                        object mappedOutput = versionMapper.ToVersion(change.Value, requestedVersion);
                        
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                            ChangeType = change.ChangeType,
                            Id = change.Id,
                            Value = mappedOutput
                        }));
                        await context.Response.WriteAsync(Environment.NewLine);
                    }
                }
                else 
                {
                    // TODO: Not Found
                    VersionedDocument versionedDocument = database.Get(id);
                    
                    object mappedOutput = versionMapper.ToVersion(versionedDocument, requestedVersion);
                    
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(mappedOutput));
                }
            });

            routeBuilder.MapPost(path, context => {

                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                string body = context.Request.Body.ReadAllText();

                // format validation
                versionMapper.Parse(body, requestedVersion);

                VersionedDocument versionedDocument = new VersionedDocument {
                    Version = requestedVersion, 
                    Document = body
                };

                database.Put(id, versionedDocument);

                return context.Response.WriteAsync("ok");
            });

            routeBuilder.MapDelete(path, async context =>
            {
                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                // TODO: Not Found
                VersionedDocument versionedDocument = database.Delete(id);
                
                object mappedOutput = versionMapper.ToVersion(versionedDocument, requestedVersion);
                
                await context.Response.WriteAsync(JsonConvert.SerializeObject(mappedOutput));
            });
            
            // TODO: Correct Put/Post behaviour

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }     
    }
}