using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roblox.Dto.Assets;
using Roblox.Dto.Persistence;
using Roblox.Services;
using Roblox.Services.Exceptions;
using ServiceProvider = Roblox.Services.ServiceProvider;
namespace Roblox.Website.Controllers
{
    [Route("/")]
    public class Datastores : ControllerBase
    {
        private bool IsRcc()
        {
            var rccAccessKey = Request.Headers.ContainsKey("accesskey") ? Request.Headers["accesskey"].ToString() : null;
            var isRcc = rccAccessKey == Configuration.RccAuthorization;
            return isRcc;
        }

        [HttpPostBypass("persistence/increment")]
        public async Task<dynamic> IncrementPersistenceAsync(long placeId, string key, string type, string scope, string target, int? value = null)
        {
            // increment?placeId=%i&key=%s&type=%s&scope=%s&target=&value=%i
            var ds = ServiceProvider.GetOrCreate<DataStoreService>();
            if (!IsRcc())
                throw new RobloxException(400, 0, "BadRequest");

            if (value == null) 
                value = int.Parse(Request.Form["value"][0]);
            string? result = await ds.Get(placeId, type, scope, key, target);

            if (result != null)
            {
                if (int.TryParse(result, out var parsedValue))
                {
                    value = parsedValue + value.Value;
                }
                else
                {
                    throw new RobloxException(400, 0, "InvalidValue");
                }
            }
            else 
            {
                throw new RobloxException(400, 0, "InvalidValue");
            }

            await ds.Set(placeId, key, type, scope, target, 31, value.ToString());
            return new
            {
                data = value,
            };
        }

        [HttpPostBypass("persistence/set")]
        public async Task<dynamic> Set(long placeId, string key, string type, string scope, string target, int valueLength)
        {
            if (!IsRcc())
                throw new RobloxException(400, 0, "BadRequest");
            var value = Request.Form["value"][0];
            await ServiceProvider.GetOrCreate<DataStoreService>()
                .Set(placeId, key, type, scope, target, valueLength, value);
            return new 
            {
                data = new 
                {
                    Value = value,
                    Scope = scope,
                    Key = key,
                    Target = target    
                }
            };
        }
        [HttpPostBypass("persistence/getSortedValues")]
        public async Task<dynamic> GetSortedPersistenceValues(long placeId, string type, string scope, string key, bool ascending, int pageSize = 50, int inclusiveMinValue = 0, int inclusiveMaxValue = 0, int? exclusiveStartKey = null)
        {
            // persistence/getSortedValues?placeId=0&type=sorted&scope=global&key=Level%5FHighscores20&pageSize=10&ascending=False"
            // persistence/set?placeId=124921244&key=BF2%5Fds%5Ftest&&type=standard&scope=global&target=BF2%5Fds%5Fkey%5Ftmp&valueLength=31
            using var ds = ServiceProvider.GetOrCreate<DataStoreService>();
            bool isEmpty = false;
            dynamic result; 
            if (!IsRcc())
                throw new RobloxException(403, 0, "BadRequest");
            if (pageSize > 100)
                throw new RobloxException(400, 0, "PageSizeTooLarge");
            if (type != "sorted")
                throw new RobloxException(400, 0, "TypeNotSorted");
            if (exclusiveStartKey == null)
                exclusiveStartKey = 1;	
            else if (exclusiveStartKey < 1)
                throw new RobloxException(400, 0, "InValidExclusiveStartKey");

            var res = await ds.GetOrderedEntry(placeId, key, scope);
            if (type == "sorted")
            {
                result = new List<GetKeyEntrySorted>();
                var addedTargets = new HashSet<string>();
                foreach (DataStoreEntry item in res)
                {
                    int value;

                    if (!int.TryParse(item.value, out value))
                    {
                        continue;
                    }

                    //rlly hacky but it works
                    if (value == 0 || addedTargets.Contains(item.name))
                    {
                        continue;
                    }
                    result.Add(new GetKeyEntrySorted()
                    {
                        Target = item.name,
                        Value = value,
                    });
                    addedTargets.Add(item.name);
                }
            }
            else 
            {
                result = new List<GetKeyEntry>();
                foreach (DataStoreEntry item in res)
                {
                    if (item.value == null)
                    {
                        isEmpty = true;
                        break;
                    }
                    result.Add(new GetKeyEntry()
                    {
                        Target = item.name,
                        Value = item.value,
                    });
                }
            }



            if (!ascending)
            {
                result.Reverse();
            }
            result.Sort((Comparison<dynamic>)((a, b) => b.Value.CompareTo(a.Value)));
            if (isEmpty)
            {
                result = new List<string>();
            }
            return new
            {
                data = new
                {
                    Entries = result,
                    ExclusiveStartKey = 1,
                },
            };
        }

        [HttpPostBypass("persistence/getv2")]
        public async Task<dynamic> GetPersistenceV2(long placeId, string type, string scope)
        {
            if (!IsRcc())
                throw new RobloxException(403, 0, "Unauthorized");
            int countRequest = 0;
            using var ds = ServiceProvider.GetOrCreate<DataStoreService>();
            dynamic result = new List<GetKeyEntry>();
            bool isEmpty = false;
            string qKeyscope;
            string qKeyTarget;
            string qKeyKey;
            while (true) 
            {
                qKeyscope = Request.Form[$"qkeys[{countRequest}].scope"]!;
                qKeyTarget = Request.Form[$"qkeys[{countRequest}].target"]!;
                qKeyKey = Request.Form[$"qkeys[{countRequest}].key"]!;

                if (qKeyscope == null || qKeyTarget == null || qKeyKey == null)
                    break;

                var entry = await ds.GetAllEntries(placeId, qKeyKey, scope, qKeyTarget);
                if (entry == null)
                {
                    countRequest++;
                    continue;
                }
                foreach (DataStoreEntry item in entry)
                {
                    //should never be null
                    if (String.IsNullOrEmpty(item.value))
                    {
                        isEmpty = true;
                        break;
                    }
                    result.Add(new GetKeyEntry()
                    {
                        Key = qKeyKey,
                        Scope = qKeyscope,
                        Target = qKeyTarget,
                        Value = item.value
                    });
                }
                countRequest++;
            }
            if (isEmpty)
                result = new List<string>();
            var finalData = new { data = result};
            string jsonString = JsonConvert.SerializeObject(finalData);
            Console.WriteLine(jsonString);
            return Content(jsonString, "application/json");
        }
    }
}
