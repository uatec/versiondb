using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using VersionDb.Client;

namespace VersionDb
{
    public static class VersionDbExtensions
    {
        public static void VersionDb(this IApplicationBuilder app, string typename, 
            params VersionRegistration[] versions)
        {
            VersionMapper versionMapper = new VersionMapper(versions);

            IDatabaseFactory databaseFactory = (IDatabaseFactory) app.ApplicationServices.GetService(typeof(IDatabaseFactory));
            
            IDatabase<VersionedDocument> database = databaseFactory.Build<VersionedDocument>(typename);

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
                        object mappedOutput = null;
                        
                        if ( change.Value != null ) 
                        {
                            mappedOutput = versionMapper.ToVersion(change.Value, requestedVersion);
                        }
                        
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

            routeBuilder.MapDelete(path, context =>
            {
                string id = context.GetRouteValue("id") as string;
                string requestedVersion = context.GetRouteValue("version") as string;

                // TODO: Not Found
                database.Delete(id);
                
                context.Response.StatusCode = StatusCodes.Status204NoContent;

                return Task.CompletedTask;
            });
            
            // TODO: Correct Put/Post behaviour

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }     
    }
}
