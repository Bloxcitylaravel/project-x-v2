using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Roblox.Services
{
    public class SetsService : ServiceBase
    {
        private static readonly HttpClient sharedClient = new HttpClient
        {
            BaseAddress = new Uri("https://sets.pizzaboxer.xyz/Game/Tools/InsertAsset.ashx"),
        };

        private async Task<string> RequestSetData(HttpClient httpClient, string query)
        {
            var builder = new UriBuilder(httpClient.BaseAddress)
            {
                Query = query
            };

            using HttpResponseMessage response = await httpClient.GetAsync(builder.Uri);
            response.EnsureSuccessStatusCode(); 
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> FetchSet(long? setId, bool bypassCache = false)
        {
            if (!bypassCache)
            {
                string cachedSetData = await redis.StringGetAsync($"set:{setId}:data");
                if (cachedSetData != null)
                {
                    return cachedSetData;
                }
            }

            string query = $"sid={setId}";
            string setData = await RequestSetData(sharedClient, query);
            await redis.StringSetAsync($"set:{setId}:data", setData, TimeSpan.FromDays(10));

            return setData;
        }

        private async Task<string> FetchUserSet(long? nsets = 20, string type = "user", long? userId = 1, bool bypassCache = false)
        {
            if (!bypassCache)
            {
                string cachedSetData = await redis.StringGetAsync($"set:{userId}:data");
                if (cachedSetData != null)
                {
                    return cachedSetData;
                }
            }

            string query = $"nsets={nsets}&type={type}&userid={userId}";
            string setData = await RequestSetData(sharedClient, query);
            await redis.StringSetAsync($"set:{userId}:data", setData, TimeSpan.FromDays(10));
            return setData;
        }

        public async Task<string> GrabSet(long? sid, long? nsets, string type, long? userId)
        {
            if (sid == null)
            {
                if (nsets == null || type == null || userId == null)
                    return null;

                return await FetchUserSet(nsets, type, userId);
            }

            return await FetchSet(sid);
        }
    }
}
