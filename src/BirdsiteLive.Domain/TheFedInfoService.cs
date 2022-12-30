using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BirdsiteLive.Domain
{
    public interface ITheFedInfoService
    {
        Task<List<BslInstanceInfo>> GetBslInstanceListAsync();
    }

    public class TheFedInfoService : ITheFedInfoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        #region Ctor
        public TheFedInfoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        #endregion

        public async Task<List<BslInstanceInfo>> GetBslInstanceListAsync()
        {
            var cancellationToken = CancellationToken.None;

            var result = await CallGraphQlAsync<MyResponseData>(
                new Uri("https://the-federation.info/graphql"),
                HttpMethod.Get,
                "query ($platform: String!) { nodes(platform: $platform) { host, version } }",
                new
                {
                    platform = "birdsitelive",
                },
                cancellationToken);

            var convertedResults = ConvertResults(result);
            return convertedResults;
        }

        private List<BslInstanceInfo> ConvertResults(GraphQLResponse<MyResponseData> qlData)
        {
            var results = new List<BslInstanceInfo>();

            foreach (var instanceInfo in qlData.Data.Nodes)
            {
                try
                {
                    var rawVersion = instanceInfo.Version.Split('+').First();
                    if (string.IsNullOrWhiteSpace(rawVersion)) continue;
                    var version = Version.Parse(rawVersion);
                    if(version <= new Version(0,1,0)) continue;

                    var instance = new BslInstanceInfo
                    {
                        Host = instanceInfo.Host,
                        Version = version
                    };
                    results.Add(instance);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return results;
        }

        private async Task<GraphQLResponse<TResponse>> CallGraphQlAsync<TResponse>(Uri endpoint, HttpMethod method, string query, object variables, CancellationToken cancellationToken)
        {
            var content = new StringContent(SerializeGraphQlCall(query, variables), Encoding.UTF8, "application/json");
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = method,
                Content = content,
                RequestUri = endpoint,
            };
            //add authorization headers if necessary here
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient();
            using (var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken))
            {
                //if (response.IsSuccessStatusCode)
                if (response?.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var responseString = await response.Content.ReadAsStringAsync(); //cancellationToken supported for .NET 5/6
                    return DeserializeGraphQlCall<TResponse>(responseString);
                }
                else
                {
                    throw new ApplicationException($"Unable to contact '{endpoint}': {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        private string SerializeGraphQlCall(string query, object variables)
        {
            var sb = new StringBuilder();
            var textWriter = new StringWriter(sb);
            var serializer = new JsonSerializer();
            serializer.Serialize(textWriter, new
            {
                query = query,
                variables = variables,
            });
            return sb.ToString();
        }

        private GraphQLResponse<TResponse> DeserializeGraphQlCall<TResponse>(string response)
        {
            var serializer = new JsonSerializer();
            var stringReader = new StringReader(response);
            var jsonReader = new JsonTextReader(stringReader);
            var result = serializer.Deserialize<GraphQLResponse<TResponse>>(jsonReader);
            return result;
        }
        
        private class GraphQLResponse<TResponse>
        {
            public List<GraphQLError> Errors { get; set; }
            public TResponse Data { get; set; }
        }

        private class GraphQLError
        {
            public string Message { get; set; }
            public List<GraphQLErrorLocation> Locations { get; set; }
            public List<object> Path { get; set; } //either int or string
        }

        private class GraphQLErrorLocation
        {
            public int Line { get; set; }
            public int Column { get; set; }
        }
        
        private class MyResponseData
        {
            public Node[] Nodes { get; set; }
        }

        private class Node
        {
            public string Host { get; set; }
            public string Version { get; set; }
        }
    }

    public class BslInstanceInfo
    {
        public string Host { get; set; }
        public Version Version { get; set; }
    }
}