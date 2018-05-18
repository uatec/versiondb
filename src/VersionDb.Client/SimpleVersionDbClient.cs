using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace VersionDb.Client
{
    public class SimpleVersionDbClient<T> : IVersionDbClient<T>
    {
        private readonly string typeName;
        private readonly string versionName;
        private readonly HttpClient httpClient;

        public SimpleVersionDbClient(HttpClient httpClient, string typeName, string versionName)
        {
            this.httpClient = httpClient;
            this.typeName = typeName;
            this.versionName = versionName;
        }

        public void Delete(string id)
        {
            var response = httpClient.DeleteAsync($"/{typeName}/{versionName}/{id}").Result;

            response.EnsureSuccessStatusCode();
        }

        public T Get(string id)
        {            
            var body = httpClient.GetStringAsync($"/{typeName}/{versionName}/{id}").Result;

            return JsonConvert.DeserializeObject<T>(body);
        }

        public IEnumerable<T> GetAll()
        {
            var body = httpClient.GetStringAsync($"/{typeName}/{versionName}").Result;

            return JsonConvert.DeserializeObject<IEnumerable<T>>(body);
        }

        public void Post(string id, T value)
        {
            string body = JsonConvert.SerializeObject(value);

            var response = httpClient.PostAsync($"/{typeName}/{versionName}/{id}", new StringContent(body)).Result;
            
            response.EnsureSuccessStatusCode();
        }

        public IEnumerable<Change<T>> Watch(string id)
        {
            var stream = httpClient.GetStreamAsync($"/{typeName}/{versionName}/{id}?watch=true").Result;

            using (var reader = new StreamReader(stream)) {

                while (!reader.EndOfStream) { 

                    //We are ready to read the stream
                    var currentLine = reader.ReadLine();

                    Change<T> change = JsonConvert.DeserializeObject<Change<T>>(currentLine);

                    yield return change;
                }
            }
        }

        public IEnumerable<Change<T>> Watch()
        {
            var stream = httpClient.GetStreamAsync($"/{typeName}/{versionName}?watch=true").Result;

            using (var reader = new StreamReader(stream)) {

                while (!reader.EndOfStream) { 

                    //We are ready to read the stream
                    var currentLine = reader.ReadLine();

                    Change<T> change = JsonConvert.DeserializeObject<Change<T>>(currentLine);

                    yield return change;
                }
            }
        }
    }
}
