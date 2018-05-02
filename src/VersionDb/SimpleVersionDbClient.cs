using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace VersionDb
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
            httpClient.DeleteAsync($"/{typeName}/{versionName}/{id}").Wait();
        }

        public T Get(string id)
        {            
            var body = httpClient.GetStringAsync($"/{typeName}/{versionName}/{id}").Result;

            return JsonConvert.DeserializeObject<T>(body);
        }

        public void Put(string id, T value)
        {
            string body = JsonConvert.SerializeObject(value);

            httpClient.PutAsync($"/{typeName}/{versionName}/{id}", new StringContent(body)).Wait();
        }

        public IEnumerable<Change<T>> Watch(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}
