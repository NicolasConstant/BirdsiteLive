using System.Net.Http;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using Newtonsoft.Json;

namespace BirdsiteLive.Domain
{
    public interface IActivityPubService
    {
        Task<Actor> GetUser(string objectId);
    }

    public class ActivityPubService : IActivityPubService
    {
        public async Task<Actor> GetUser(string objectId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var result = await httpClient.GetAsync(objectId);
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Actor>(content);
            }
        }
    }
}