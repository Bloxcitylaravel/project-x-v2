using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Roblox.Dto.Games;
using Roblox.Dto.Persistence;
using Roblox.Dto.Users;
using MVC = Microsoft.AspNetCore.Mvc;
using Roblox.Libraries.Assets;
using Roblox.Libraries.FastFlag;
using Roblox.Libraries.RobloxApi;
using Roblox.Logging;
using Roblox.Services.Exceptions;
using BadRequestException = Roblox.Exceptions.BadRequestException;
using Roblox.Models.Assets;
using Roblox.Models.GameServer;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Website.Filters;
using Roblox.Website.Middleware;
using Roblox.Website.WebsiteModels.Asset;
using Roblox.Website.WebsiteModels.Games;
using HttpGet = Roblox.Website.Controllers.HttpGetBypassAttribute;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MultiGetEntry = Roblox.Dto.Assets.MultiGetEntry;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
using ServiceProvider = Roblox.Services.ServiceProvider;
using Type = Roblox.Models.Assets.Type;
using Microsoft.AspNetCore.Mvc;
using Roblox.Website.WebsiteModels.Authentication;
using System.Text.RegularExpressions;
using InfluxDB.Client.Core.Exceptions;
using Roblox.Exceptions;
using Roblox.Website.Pages;
using System.IO.Compression;
using Roblox.Models;
using Roblox.Dto.Assets;
using Roblox.Models.Games;
namespace Roblox.Website.Controllers
{
    [MVC.ApiController]
    [MVC.Route("/")]
    public class BypassController : ControllerBase
    {
        [HttpGetBypass("internal/release-metadata")]
        public dynamic GetReleaseMetaData([Required] string requester)
        {
            throw new RobloxException(RobloxException.BadRequest, 0, "BadRequest");
        }
/*
        [HttpGetBypass("asset/shader")]
        public async Task<MVC.FileResult> GetShaderAsset(long id)
        {
            var isMaterialOrShader = BypassControllerMetadata.materialAndShaderAssetIds.Contains(id);
            if (!isMaterialOrShader)
            {
                // Would redirect but that could lead to infinite loop.
                // Just throw instead
                throw new RobloxException(400, 0, "BadRequest");
            }

            var assetId = id;
            try
            {
                var ourId = await services.assets.GetAssetIdFromRobloxAssetId(assetId);
                assetId = ourId;
            }
            catch (RecordNotFoundException)
            {
                // Doesn't exist yet, so create it
                var migrationResult = await MigrateItem.MigrateItemFromRoblox(assetId.ToString(), false, null, default, new ProductDataResponse()
                {
                    Name = "ShaderConversion" + id,
                    AssetTypeId = Type.Special, // Image
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Description = "ShaderConversion1.0",
                });
                assetId = migrationResult.assetId;
            }
            
            var latestVersion = await services.assets.GetLatestAssetVersion(assetId);
            if (latestVersion.contentUrl is null)
            {
                throw new RobloxException(403, 0, "Forbidden"); // ?
            }
            // These files are large, encourage clients to cache them
            HttpContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(360),
            }.ToString();
            var assetContent = await services.assets.GetAssetContent(latestVersion.contentUrl);
            return File(assetContent, "application/binary");
        }
        */
        public bool IsRcc()
        {
            var rccAccessKey = Request.Headers.ContainsKey("accesskey") ? Request.Headers["accesskey"].ToString() : null;
            var isRcc = rccAccessKey == Configuration.RccAuthorization;
            return isRcc;
        }

        [HttpGetBypass("v2/asset")]
        [HttpGetBypass("v1/asset")]
        [HttpGetBypass("asset")]
        [HttpPostBypass("v1/asset")]
        [HttpPostBypass("asset")]
        public async Task<MVC.ActionResult> GetAssetById(long? playerId, long id, long? assetversionid = null)
        {
            

            /*
            This is from corescripts from 2017 for more context
            
            local CUSTOM_ICONS = {	-- Admins with special icons
            ['7210880'] = 'rbxassetid://134032333', -- Jeditkacheff
            ['13268404'] = 'rbxassetid://113059239', -- Sorcus
            ['261'] = 'rbxassetid://105897927', -- shedlestky
            ['20396599'] = 'rbxassetid://161078086', -- Robloxsai
            }
            if (playerId == 20396599)
               id = 10812;
            if(id == 161078086){
                id = 10812;
            }
            
            */

            HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            HttpContext.Response.Headers.Add("Pragma", "no-cache");
            HttpContext.Response.Headers.Add("Expires", "-1");
            HttpContext.Response.Headers.Add("ExpiresAbsolute", "0");
            // TODO: This endpoint needs to be updated to return a URL to the asset, not the asset itself.
            // The reason for this is so that cloudflare can cache assets without caching the response of this endpoint, which might be different depending on the client making the request (e.g. under 18 user, over 18 user, rcc, etc).
            if(id == 507766388)
            {
                return PhysicalFile(@"C:\ProjectX\services\Roblox\FixJitter\507766388.rbxm", "application/octet-stream");  
            }
            else if(id == 507766666)
            {
                return PhysicalFile(@"C:\ProjectX\services\Roblox\FixJitter\507766666.rbxm", "application/octet-stream");      
            }
            if(assetversionid != null)
            {
                id = (long)assetversionid;
            }
            var is18OrOver = false;
            if (userSession != null)
            {
                is18OrOver = await services.users.Is18Plus(safeUserSession.userId);
            }

            // TEMPORARY UNTIL AUTH WORKS ON STUDIO! REMEMBER TO REMOVE
            if (HttpContext.Request.Headers.ContainsKey("RbxTempBypassFor18PlusAssets"))
            {
                is18OrOver = true;
            }

            var assetId = id;
            var invalidIdKey = "InvalidAssetIdForConversionV1:" + assetId;
            // Opt
            if (Services.Cache.distributed.StringGetMemory(invalidIdKey) != null)
                throw new RobloxException(400, 0, "Asset is invalid or does not exist");
            
            var isBotRequest = Request.Headers["bot-auth"].ToString() == Roblox.Configuration.BotAuthorization;
            var isLoggedIn = userSession != null;
            var encryptionEnabled = !isBotRequest; // bots can't handle encryption :(
#if DEBUG == false
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault()?.ToLower();
            var requester = Request.Headers["Requester"].FirstOrDefault()?.ToLower();
            if (!isBotRequest && !isLoggedIn && (userAgent == null || requester == null || (requester != "client" && requester != "server") || !BypassControllerMetadata.allowedUserAgents.Contains(userAgent)))
            {
                throw new BadRequestException();
            }
#endif

            var isMaterialOrShader = BypassControllerMetadata.materialAndShaderAssetIds.Contains(assetId);
            if (isMaterialOrShader)
            {
                return new MVC.RedirectResult("/asset/shader?id=" + assetId);
            }

            var isRcc = IsRcc();

            if (isRcc)
                encryptionEnabled = false;
#if DEBUG
            encryptionEnabled = false;
#endif
            MultiGetEntry details;
            try 
            {
                details = await services.assets.GetAssetCatalogInfo(assetId);
            } 
            catch (RecordNotFoundException) 
            {
                try
                {
                    var ourId = await services.assets.GetAssetIdFromRobloxAssetId(assetId);
                    assetId = ourId;
                }
                catch (RecordNotFoundException)
                {
                    /*if (await Services.Cache.distributed.StringGetAsync(invalidIdKey) != null)
                        throw new RobloxException(400, 0, "Asset is invalid or does not exist");
                    
                    try
                    {
                        // Doesn't exist yet, so create it
                        var migrationResult = await MigrateItem.MigrateItemFromRoblox(assetId.ToString(), false, null,
                            new List<Type>()
                            {
                                Type.Image,
                                Type.Audio,
                                Type.Mesh,
                                Type.Lua,
                                Type.Model,
                                Type.Decal,
                                Type.Animation,
                                Type.SolidModel,
                                Type.MeshPart,
                                Type.ClimbAnimation,
                                Type.DeathAnimation,
                                Type.FallAnimation,
                                Type.IdleAnimation,
                                Type.JumpAnimation,
                                Type.RunAnimation,
                                Type.SwimAnimation,
                                Type.WalkAnimation,
                                Type.PoseAnimation,
                            }, default, default, true);
                        assetId = migrationResult.assetId;
                    }
                    catch (AssetTypeNotAllowedException)
                    {
                        // TODO: permanently insert as invalid for AssetTypeNotAllowedException in a table
                        await Services.Cache.distributed.StringSetAsync(invalidIdKey,
                            "{}", TimeSpan.FromDays(7));
                        throw new RobloxException(400, 0, "Asset is invalid or does not exist");
                    }
                    catch (Exception e)
                    {
                        // temporary failure? mark as invalid, but only temporarily
                        Writer.Info(LogGroup.AssetDelivery, "Failed to migrate asset " + assetId + " - " + e.Message + "\n" + e.StackTrace);
                        await Services.Cache.distributed.StringSetAsync(invalidIdKey,
                            "{}", TimeSpan.FromMinutes(1));
                        throw new RobloxException(400, 0, "Asset is invalid or does not exist");
                    }
                    */
                    return Redirect($"https://assetdelivery.roblox.com/v1/asset/?id={assetId}");
                }
                details = await services.assets.GetAssetCatalogInfo(assetId);
            }
            if (details.is18Plus && !isRcc && !isBotRequest && !is18OrOver)
                throw new RobloxException(400, 0, "AssetTemporarilyUnavailable");
            if (details.moderationStatus != ModerationStatus.ReviewApproved && !isRcc && !isBotRequest)
                throw new RobloxException(403, 0, "Asset not approved for requester");
            dynamic assetVersion = await services.assets.GetLatestAssetVersion(assetId);
            /*
            if (version != null)
            {
                assetVersion = await services.assets.GetSpecificAssetVersion(assetId, (long)version);
            }
            else
            {
                assetVersion = 
            }
            */
            Stream? assetContent = null;
            switch (details.assetType)
            {
                // Special types
                case Roblox.Models.Assets.Type.TeeShirt:
                    return new MVC.FileContentResult(Encoding.UTF8.GetBytes(ContentFormatters.GetTeeShirt(assetVersion.contentId)), "application/binary");
                case Models.Assets.Type.Shirt:
                    return new MVC.FileContentResult(Encoding.UTF8.GetBytes(ContentFormatters.GetShirt(assetVersion.contentId)), "application/binary");
                case Models.Assets.Type.Pants:
                    return new MVC.FileContentResult(Encoding.UTF8.GetBytes(ContentFormatters.GetPants(assetVersion.contentId)), "application/binary");
                // Types that require no authentication and aren't encrypted
                case Models.Assets.Type.Image:
                case Models.Assets.Type.Special:
                    if (assetVersion.contentUrl != null)
                        assetContent = await services.assets.GetAssetContent(assetVersion.contentUrl);
                    // encryptionEnabled = false;
                    break;
                // Types that require no authentication
                case Models.Assets.Type.Audio:
                case Models.Assets.Type.Mesh:
                case Models.Assets.Type.Hat:
                case Models.Assets.Type.Model:
                case Models.Assets.Type.Decal:
                case Models.Assets.Type.Head:
                case Models.Assets.Type.Face:
                case Models.Assets.Type.Gear:
                case Models.Assets.Type.Badge:
                case Models.Assets.Type.Animation:
                case Models.Assets.Type.Torso:
                case Models.Assets.Type.RightArm:
                case Models.Assets.Type.LeftArm:
                case Models.Assets.Type.RightLeg:
                case Models.Assets.Type.LeftLeg:
                case Models.Assets.Type.Package:
                case Models.Assets.Type.GamePass:
                case Models.Assets.Type.Plugin: // TODO: do plugins need auth?
                case Models.Assets.Type.MeshPart:
                case Models.Assets.Type.HairAccessory:
                case Models.Assets.Type.FaceAccessory:
                case Models.Assets.Type.NeckAccessory:
                case Models.Assets.Type.ShoulderAccessory:
                case Models.Assets.Type.FrontAccessory:
                case Models.Assets.Type.BackAccessory:
                case Models.Assets.Type.WaistAccessory:
                case Models.Assets.Type.ClimbAnimation:
                case Models.Assets.Type.DeathAnimation:
                case Models.Assets.Type.FallAnimation:
                case Models.Assets.Type.IdleAnimation:
                case Models.Assets.Type.JumpAnimation:
                case Models.Assets.Type.RunAnimation:
                case Models.Assets.Type.SwimAnimation:
                case Models.Assets.Type.WalkAnimation:
                case Models.Assets.Type.PoseAnimation:
                case Models.Assets.Type.SolidModel:
                case Models.Assets.Type.Video:
                    if (assetVersion.contentUrl is null)
                        throw new RobloxException(400, 0, "BadRequest"); // todo: should we log this?
                    //if (details.assetType == Models.Assets.Type.Audio)
                    //we dont have a web client so we dont need this anymore
                    //{
                        // Convert to WAV file since that's what web client requires
                        //assetContent = await services.assets.GetAudioContentAsWav(assetId, assetVersion.contentUrl);
                    //}
                    else
                    {
                        assetContent = await services.assets.GetAssetContent(assetVersion.contentUrl);
                    }
                    break;
                default:
                    // anything else requires auth
                    var ok = false;
                    if (isRcc)
                    {
                        encryptionEnabled = false;
                        var placeIdHeader = Request.Headers["roblox-place-id"].ToString();
                        long placeId = 0;
                        if (!string.IsNullOrEmpty(placeIdHeader))
                        {
                            try
                            {
                                placeId = long.Parse(Request.Headers["roblox-place-id"].ToString());
                            }
                            catch (FormatException)
                            {
                                // Ignore
                            }
                        }
                        // if rcc is trying to access current place, allow through
                        ok = (placeId == assetId);
                        // If game server is trying to load a new place (current placeId is empty), then allow it
                        if (!ok && details.assetType == Models.Assets.Type.Place && placeId == 0)
                        {
                            // Game server is trying to load, so allow it
                            ok = true;
                        }
                        // If rcc is making the request, but it's not for a place, validate the request:
                        if (!ok)
                        {
                            // Check permissions
                            var placeDetails = await services.assets.GetAssetCatalogInfo(placeId);
                            if (placeDetails.creatorType == details.creatorType &&
                                placeDetails.creatorTargetId == details.creatorTargetId)
                            {
                                // We are authorized
                                ok = true;
                            }
                        }
                    }
                    else
                    {
                        // It's not RCC making the request. are we authorized?
                        if (userSession != null)
                        {
                            // Use current user as access check
                            ok = await services.assets.CanUserModifyItem(assetId, safeUserSession.userId);
                            if (!ok)
                            {
                                // Note that all users have access to "Roblox"'s content for legacy reasons
                                ok = (details.creatorType == CreatorType.User && details.creatorTargetId == 1);
                            }
#if DEBUG
                            // If staff, allow access in debug builds
                            if (UsersService.IsUserStaff(userSession.userId))
                            {
                                ok = true;
                            }
#endif
                            // Don't encrypt assets being sent to authorized users - they could be trying to download their own place to give to a friend or something
                            if (ok)
                            {
                                encryptionEnabled = false;
                            }
                        }
                    }

                    if (ok && assetVersion.contentUrl != null)
                    {
                        assetContent = await services.assets.GetAssetContent(assetVersion.contentUrl);
                    }

                    break;
            }

            if (assetContent != null)
            {
                return File(assetContent, "application/binary");
            }

            Console.WriteLine("[info] got BadRequest on /asset/ endpoint");
            throw new BadRequestException();
        }
        [HttpPostBypass("asset/batch")]
        [HttpPostBypass("v1/assets/batch")]
        public async Task<IActionResult> AssetBatch()
        {
            List<BatchAssetRequest> requestData;
            bool isGzip = Request.Headers["Content-Encoding"].ToString() == "gzip";
            
            if (isGzip)
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var requestStream = Request.Body)
                    {
                        using (var gzipStream = new GZipStream(requestStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                    }
                    decompressedStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        var json = await reader.ReadToEndAsync();
                        Console.WriteLine(json);
                        requestData = JsonSerializer.Deserialize<List<BatchAssetRequest>>(json);
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    Console.WriteLine(json);
                    requestData = JsonSerializer.Deserialize<List<BatchAssetRequest>>(json);
                }
            }
            if (requestData == null)
            {
                throw new BadRequestException();
            }
            var assetReturnInfo = new List<object>();
            foreach (var request in requestData)
            {
                Console.WriteLine(request.assetId);
                assetReturnInfo.Add(new
                {
                    Location = $"{Configuration.BaseUrl}/v1/asset?id={request.assetId}",
                    RequestId = request.requestId,
                    IsHashDynamic = true,
                    IsCopyrightProtected = true, 
                    IsArchived = false,
                });
            }

            return Content(JsonSerializer.Serialize(assetReturnInfo), "application/json");
        }
        [HttpGetBypass("universes/get-universe-containing-place")]
        public async Task<dynamic> GetUniverse(long placeid)
        {
            return new 
            {
                UniverseId = await services.games.GetUniverseId(placeid)
            };
        }
        [HttpGetBypass("Game/LoadPlaceInfo.ashx")]
        public async Task<string> LoadPlaceInfo(long PlaceId)
        {
            var details = await services.assets.GetAssetCatalogInfo(PlaceId);
            // this is just easier for me then using replace all the time on every pcall
            string httpsToHttp = Configuration.BaseUrl.Replace("https", "http");
            string luaCode = $@"
                            pcall(function() game:SetCreatorID({details.creatorTargetId}, Enum.CreatorType.User) end);
                            pcall(function() game:GetService(""SocialService""):SetFriendUrl(""{httpsToHttp}/Game/LuaWebService/HandleSocialRequest.ashx?method=IsFriendsWith&playerid=%d&userid=%d"") end);
                            pcall(function() game:GetService(""SocialService""):SetBestFriendUrl(""{httpsToHttp}/Game/LuaWebService/HandleSocialRequest.ashx?method=IsBestFriendsWith&playerid=%d&userid=%d"") end);
                            pcall(function() game:GetService(""SocialService""):SetGroupUrl(""{httpsToHttp}/Game/LuaWebService/HandleSocialRequest.ashx?method=IsInGroup&playerid=%d&groupid=%d"") end);
                            pcall(function() game:GetService(""SocialService""):SetGroupRankUrl(""{httpsToHttp}/Game/LuaWebService/HandleSocialRequest.ashx?method=GetGroupRank&playerid=%d&groupid=%d"") end);
                            pcall(function() game:GetService(""SocialService""):SetGroupRoleUrl(""{httpsToHttp}/Game/LuaWebService/HandleSocialRequest.ashx?method=GetGroupRole&playerid=%d&groupid=%d"") end);
                            pcall(function() game:GetService(""GamePassService""):SetPlayerHasPassUrl(""{httpsToHttp}/Game/GamePass/GamePassHandler.ashx?Action=HasPass&UserID=%d&PassID=%d"") end);
            ";

            string[] lines = luaCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimStart();
            }

            luaCode = string.Join("\n", lines);
            string SignedScript = services.sign.SignStringResponseForClientFromPrivateKey(luaCode, true);
            return SignedScript;

        }
        [HttpGetBypass("Game/GamePass/GamePassHandler.ashx")]
        public async Task<string> GamePassHandler(string Action, long UserID, long PassID)
        {
            if (Action == "HasPass")
            {
                var has = await services.users.GetUserAssets(UserID, PassID);
                return has.Any() ? "True" : "False";
            }

            throw new NotImplementedException();
        }

        [HttpGetBypass("Game/LuaWebService/HandleSocialRequest.ashx")]
        public async Task<string> LuaSocialRequest([Required, MVC.FromQuery] string method, long? playerid = null, long? groupid = null, long? userid = null)
        {
            // TODO: Implement these
            method = method.ToLower();
            if (method == "isingroup" && playerid != null && groupid != null)
            {
                bool isInGroup = false;
                try
                {
                    if (groupid == 1200769 && await StaffFilter.IsStaff(playerid ?? 0))
                    {
                        isInGroup = true;
                    }
                    var group = await services.groups.GetUserRoleInGroup((long) groupid, (long)playerid);
                    if (group.rank != 0)
                        isInGroup = true;
                }
                catch (Exception)
                {
                    
                }

                return "<Value Type=\"boolean\">"+(isInGroup ? "true" : "false")+"</Value>";
            }

            if (method == "getgrouprank" && playerid != null && groupid != null)
            {
                int rank = 0;
                try
                {
                    var group = await services.groups.GetUserRoleInGroup((long) groupid, (long) playerid);
                    rank = group.rank;
                }
                catch (Exception)
                {
                    
                }

                return "<Value Type=\"integer\">"+rank+"</Value>";
            }

            if (method == "getgrouprole" && playerid != null && groupid != null)
            {
                var groups = await services.groups.GetAllRolesForUser((long) playerid);
                foreach (var group in groups)
                {
                    if (group.groupId == groupid)
                    {
                        return group.name;
                    }
                }

                return "Guest";
            }

            if (method == "isfriendswith" && playerid != null && userid != null)
            {
                var status = (await services.friends.MultiGetFriendshipStatus((long) playerid, new[] {(long) userid})).FirstOrDefault();
                if (status != null && status.status == "Friends")
                {
                    return "<Value Type=\"boolean\">True</Value>";
                }
                return "<Value Type=\"boolean\">False</Value>";

            }

            if (method == "isbestfriendswith")
            {
                return "<Value Type\"boolean\">False</value>";
            }

            throw new NotImplementedException();
        }

        [HttpGetBypass("v2/users/{userId:long}/groups/roles")]
        public async Task<RobloxCollection<dynamic>> GetUserGroupRoles(long userId)
        {
            var roles = await services.groups.GetAllRolesForUser(userId);
            var result = new List<dynamic>();
            foreach (var role in roles)
            {
                var groupDetails = await services.groups.GetGroupById(role.groupId);
                result.Add(new
                {
                    group = new
                    {
                        id = groupDetails.id,
                        name = groupDetails.name,
                        memberCount = groupDetails.memberCount,
                    },
                    role = role,
                });
            }
            if (await StaffFilter.IsStaff(userId))
            {
                result.Add(new
                {
                    group = new
                    {
                        id = 1200769,
                        name = "Project X Admin",
                        memberCount = 100,
                    },
                    role = new
                    {
                        id = 1,
                        name = "Admin",
                        rank = 100
                    }
                });
            }
            return new()
            {
                data = result,
            };
        }
        [HttpGetBypass("/auth/submit")]
        public MVC.RedirectResult SubmitAuth(string auth)
        {
            return new MVC.RedirectResult("/");
        }

        [HttpPostBypass("/v1/join-game")]
        public async Task<PlaceLaunchResponse> JoinGameMobile([FromBody] JoinGame request)
        {
            long year = await services.games.GetYear(request.placeId);
            if (year != 2020 && year != 2021)
            {
                return new PlaceLaunchResponse()
                {
                    status = (int)JoinStatus.Error,
                    message = "An error occured while starting the game."
                };
            }
            var placeLauncherRequest = new PlaceLaunchRequest
            {
                request = "RequestGame",
                placeId = request.placeId,
                userId = safeUserSession.userId,
                username = safeUserSession.username,
                special = true
            };
            return await services.placeLauncherFactory.PlaceLauncherAsync(placeLauncherRequest);
        }
        [HttpPostBypass("/game/PlaceLauncher.ashx")]
        [HttpGetBypass("/game/PlaceLauncher.ashx")]
        public async Task<PlaceLaunchResponse> PlaceLaunch([FromQuery] PlaceLaunchRequest Placelauncher)
        {     
            FeatureFlags.FeatureCheck(FeatureFlag.GamesEnabled, FeatureFlag.GameJoinEnabled);
            long year = await services.games.GetYear(Placelauncher.placeId);
        
            if (!ApplicationGuardMiddleware.IsRoblox(Request) || year == 2016){
                return new PlaceLaunchResponse()
                {
                    status = (int)JoinStatus.Error,
                    message = "An error occured while starting the game."
                };
            }
            if(userSession == null)
            {
                return new PlaceLaunchResponse()
                {
                    status = (int)JoinStatus.Unauthorized,
                    message = "You are not authorized to join"
                };
            }
            Placelauncher.cookie = HttpContext.Request.Cookies[".ROBLOSECURITY"].ToString();
            return await services.placeLauncherFactory.PlaceLauncherAsync(Placelauncher);
        }

        public static long startUserId {get;set;} = 30;
#if DEBUG
        [HttpGetBypass("/game/get-join-script-debug")]
        public async Task<dynamic> GetJoinScriptDebug(long placeId, long userId = 12)
        {
            //startUserId = 12;
            var result = services.gameServer.CreateTicket(startUserId, placeId, GetIP());
            startUserId++;
            return new
            {
                placeLauncher = $"{Configuration.BaseUrl}/placelauncher.ashx?ticket={HttpUtility.UrlEncode(result)}",
                authenticationTicket = result,
            };
        }
#endif
        [HttpPostBypass("login/RequestAuth.ashx")]
        [HttpGetBypass("login/RequestAuth.ashx")]
        public ActionResult<dynamic?> StudioRequestAuth()
        {
            if (userSession == null)
            {
                return Unauthorized("User is not authorized.");
            }

            string? cookie = HttpContext.Request.Cookies[".ROBLOSECURITY"];
            return Ok($"{Configuration.BaseUrl}/Login/Negotiate.ashx?suggest={cookie}");
        }

        [HttpGetBypass("getserverinfo")]
        public async Task<dynamic> GetServerInfo(string ip)
        {
            return await services.games.GetInfoFromIp(ip);
        }

        [HttpGetBypass("getrichpresence")]
        public async Task<dynamic> GetRichPresenceInfo(long userId, long placeId)
        {
            string JobId = "";
            int playerCount = 0;
            try
            {
                if (userId != 0)
                {
                    JobId = await services.gameServer.GetJobIdByUserId(userId);
                    var currentPlayerCount = await services.gameServer.GetGameServerPlayers(JobId);
                    playerCount = currentPlayerCount.Count();
                }
            }
            catch (Exception)
            {
                playerCount = 0;
            }

            long maxplayers = await services.games.GetMaxPlayerCount(placeId);
            var placeInfo = await services.assets.GetAssetCatalogInfo(placeId);
            long year = await services.games.GetYear(placeId);
            return new 
            {
                Creator = placeInfo.creatorName,
                Name = placeInfo.name,
                Year = year,
                MaxPlayers = maxplayers,
                PartyId = Guid.NewGuid().ToString(),
                CurrentPlayers = playerCount,
            };
        }
        [HttpGetBypass("My/Places.aspx")]
        public ActionResult<dynamic?> MyPlaces()
        {
            return Ok();
        }
        [HttpGetBypass("games/list-json")]
        public IActionResult SillyGameJson()
        {
            dynamic gameDetail = new System.Dynamic.ExpandoObject();

            gameDetail.CreatorID = 20;
            gameDetail.CreatorName = "ass";
            gameDetail.CreatorUrl = "https://www.projex.zip/users/20/profile";
            gameDetail.Plays = 1;
            gameDetail.Price = 0;
            gameDetail.ProductID = 0;
            gameDetail.IsOwned = false;
            gameDetail.IsVotingEnabled = true;
            gameDetail.TotalUpVotes = 69;
            gameDetail.TotalDownVotes = 69;
            gameDetail.TotalBought = 69;
            gameDetail.UniverseID = 189;
            gameDetail.HasErrorOcurred = false;
            gameDetail.GameDetailReferralUrl = "https://www.projex.zip/games/189/Natural-Disaster-Survival";
            gameDetail.Url = "https://www.projex.zip/images/thumbnails/e70dd27c44ca8bebebb14f48fbba28c5b5a2ba79ebb1e3c820c3a1e84fc8aed5.png";
            gameDetail.RetryUrl = null;
            gameDetail.Final = true;
            gameDetail.Name = "Natural Disaster Survival";
            gameDetail.PlaceID = 189;
            gameDetail.PlayerCount = 10347;
            gameDetail.ImageId = 2311;

            List<dynamic> gameDetailList = new List<dynamic>(); 
            gameDetailList.Add(gameDetail); 
            gameDetailList.Add(gameDetail); 
            gameDetailList.Add(gameDetail); 
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(gameDetailList); 
            return Content(jsonString, "application/json");
        }
        [HttpGetBypass("game/GetCurrentUser.ashx")]
        public IActionResult GetUserId()
        {
            if (userSession == null){
               return Ok("Bad Request");
            }
            string userIdAsString = userSession.userId.ToString();
            return Content(userIdAsString, "text/plain");
        }
        [HttpGetBypass("/mobileapi/check-app-version")]
        [HttpPostBypass("/mobileapi/check-app-version")]
        public ActionResult<dynamic> CheckAppVersion()
        {

            dynamic data = new { UpgradeAction = "None" };
            var json = new
            { data = data };

            string jsonString = JsonConvert.SerializeObject(json);
            return Content(jsonString, "application/json");
        }

        [HttpGetBypass("download2")]
        public async Task<dynamic> DownloadPage()
        {
            //do this for anti reporting shit
            if(userSession == null)
            {
                return Redirect("/auth/home");
            }
            return Content(await System.IO.File.ReadAllTextAsync("download.html"), "text/html");
        }
        [HttpGetBypass("set-year")]
        public async Task<dynamic> TaskAsync (long universeId, int year)
        {
            var place = await services.games.GetRootPlaceId(universeId);
            await services.assets.ValidatePermissions(place, safeUserSession.userId);
            await services.games.SetYear(place, year);
            return "ok";
        } 
        [HttpGetBypass("login/negotiate.ashx"), HttpGetBypass("login/negotiateasync.ashx"), HttpPostBypass("login/negotiate.ashx")]
        public void Negotiate([Required, FromQuery] string suggest)
        {
            HttpContext.Response.Cookies.Append(".ROBLOSECURITY", suggest, new CookieOptions
            {
                Domain = ".projex.zip",
                Secure = false,
                Expires = DateTimeOffset.Now.Add(TimeSpan.FromDays(364)),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
            });
        }

        [HttpPostBypass("game/join.ashx")]
        [HttpGetBypass("game/join.ashx")]
        public async Task<dynamic> JoinGame(string jobId, long placeId, bool GenerateTeleportJoin = false)
        {
            var jobPlayers = await services.gameServer.GetGameServerPlayers(jobId);
            PlaceEntry uni = (await services.games.MultiGetPlaceDetails(new[] { placeId })).First();
            string username = safeUserSession.username;
            long userId = safeUserSession.userId;
            string membership;
            var membership2 = await services.users.GetUserMembership(userId);
            DateTime currentUtcDateTime = DateTime.UtcNow;
            string formattedDateTime = currentUtcDateTime.ToString("M/d/yyyy h:mm:ss tt");
            string finalTicket;
            string characterAppearanceUrl = $"{Configuration.BaseUrl.Replace("https", "http")}/v1.1/avatar-fetch?userId={userId}&placeId={placeId}";
            if (jobPlayers.Count() >= uni.maxPlayerCount)
            {
                return new
                {
                    error = "The requested game is full",
                    status = 5
                };
            }
            var userInfo = await services.users.GetUserById(userSession!.userId);
            Console.WriteLine(username);
            var accountAgeDays = DateTime.UtcNow.Subtract(userInfo.created).Days;
            if (membership2 == null)
            {
                membership = "None";
            }
            else
            {
                membership = (int)membership2!.membershipType == 4 ? "Premium" : (int)membership2!.membershipType == 3 ? "OutrageousBuildersClub" : (int)membership2.membershipType == 2 ? "TurboBuildersClub" : (int)membership2.membershipType == 1 ? "BuildersClub" : "None";
            }
            Console.WriteLine(membership);
            if(uni.year != 2020 && uni.year != 2021 && membership == "Premium")
            {
                membership = "OutrageousBuildersClub";
            }
            switch (uni.year)
            {
                case 2015:
                case 2016:
                    characterAppearanceUrl = $"{Configuration.BaseUrl}/Asset/CharacterFetch.ashx?userId={userId}";
                    finalTicket = services.sign.GenerateClientTicketV1(userId, username, jobId, characterAppearanceUrl);
                    break;
                case 2017:                  
                    finalTicket = services.sign.GenerateClientTicketV1(userId, username, jobId, characterAppearanceUrl);
                    break;
                case 2018:
                case 2019:
                    finalTicket = services.sign.GenerateClientTicketV2(userId, username, jobId, characterAppearanceUrl);
                    break;
                case 2020:
                    characterAppearanceUrl = $"http://www.projex.zip/v1/avatar-fetch?userId={placeId}&placeId={placeId}";
                    finalTicket = services.sign.GenerateClientTicketV4(userId, username, characterAppearanceUrl, membership, jobId, formattedDateTime, accountAgeDays, placeId);
                    break;
                case 2021:
                    characterAppearanceUrl = $"http://www.projex.zip/v1/avatar-fetch?userId={placeId}&placeId={placeId}";
                    finalTicket = services.sign.GenerateClientTicketV4(userId, username, characterAppearanceUrl, membership, jobId, formattedDateTime, accountAgeDays, placeId);
                    break;
                default:
                    throw new InvalidOperationException($"This year does not exist: {uni.year}");
            }
            

            FeatureFlags.FeatureCheck(FeatureFlag.GamesEnabled, FeatureFlag.GameJoinEnabled);
            dynamic joinScript = null;
            try 
            {
                //needed for matchmaking so we can select the best route
                //string ip = GetRequesterIpRaw(HttpContext);
                joinScript = await services.games.GetJoinScript((long)uni.year, username, userId, jobId, placeId, uni.universeId, uni.builderId, characterAppearanceUrl, finalTicket, membership, accountAgeDays, GenerateTeleportJoin, Request.Cookies[".ROBLOSECURITY"].ToString());
            }
            catch (Exception e)
            {
                throw new BadRequestException(1, "Couldn't find gameserver");
            }
            return services.games.SignJoinScript((long)uni.year, joinScript);
        }
        [HttpGetBypass("GenerateVersion")]
        public string GenerateVersion()
        {
            return $"version-{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16)}";
        }
        [HttpGetBypass("GenerateAuthString")]
        public string GenerateAuthString()
        {
            string twoGuid = Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "");
            return "PJX-" + twoGuid;
        }
        [HttpGetBypass("Asset/CharacterFetch.ashx")]
        public async Task<string> CharacterFetchASHX(long userId)
        {
            var assets = await services.avatar.GetWornAssets(userId);
            return $"{Configuration.BaseUrl}/Asset/BodyColors.ashx?userId={userId};{string.Join(";", assets.Select(c => Configuration.BaseUrl + "/Asset/?id=" + c))}";
        }

        [HttpGetBypass("my/settings/json")]
        public async Task<dynamic> SettingsJsonA()
        {
            var userInfo = await services.users.GetUserById(safeUserSession.userId);
            string membership;
            bool isAdmin = await StaffFilter.IsStaff(safeUserSession.userId);
            var membership2 = await services.users.GetUserMembership(safeUserSession.userId);
            if (membership2 == null)
            {
                membership = "None";
            }
            else
            {
                membership = (int)membership2!.membershipType == 4 ? "Premium" : (int)membership2!.membershipType == 3 ? "OutrageousBuildersClub" : (int)membership2.membershipType == 2 ? "TurboBuildersClub" : (int)membership2.membershipType == 1 ? "BuildersClub" : "None";
            }
            return new
            {
                ChangeUsernameEnabled = true,
                IsAdmin = isAdmin,
                UserId = safeUserSession.userId,
                Name = safeUserSession.username,
                DisplayName = safeUserSession.username,
                IsEmailOnFile = true,
                IsEmailVerified = true,
                IsPhoneFeatureEnabled = true,
                RobuxRemainingForUsernameChange = 0,
                PreviousUserNames = "",
                UseSuperSafePrivacyMode = false,
                IsSuperSafeModeEnabledForPrivacySetting = false,
                UseSuperSafeChat = false,
                IsAppChatSettingEnabled = true,
                IsGameChatSettingEnabled = true,
                IsAccountPrivacySettingsV2Enabled = true,
                IsSetPasswordNotificationEnabled = false,
                ChangePasswordRequiresTwoStepVerification = false,
                ChangeEmailRequiresTwoStepVerification = false,
                UserEmail = "projectx@projex.zip",
                UserEmailMasked = true,
                UserEmailVerified = true,
                CanHideInventory = true,
                CanTrade = false,
                MissingParentEmail = false,
                IsUpdateEmailSectionShown = true,
                IsUnder13UpdateEmailMessageSectionShown = false,
                IsUserConnectedToFacebook = false,
                IsTwoStepToggleEnabled = false,
                AgeBracket = 0,
                UserAbove13 = true,
                ClientIpAddress = GetRequesterIpRaw(HttpContext),
                AccountAgeInDays = DateTime.UtcNow.Subtract(userInfo.created).Days,
                IsOBC = membership == "OutrageousBuildersClub",
                IsTBC = membership == "TurboBuildersClub",
                IsAnyBC = membership != "None",
                IsPremium = false,
                IsBcRenewalMembership = false,
                BcExpireDate = "/Date(-0)/",
                BcRenewalPeriod = (string?)null,
                BcLevel = (int?)null,
                HasCurrencyOperationError = false,
                CurrencyOperationErrorMessage = (string?)null,
                BlockedUsersModel = new
                {
                    BlockedUserIds = new List<int>() { },
                    BlockedUsers = new List<string>() { },
                    MaxBlockedUsers = 50,
                    Total = 1,
                    Page = 1
                },
                Tab = (string?)null,
                ChangePassword = false,
                IsAccountPinEnabled = true,
                IsAccountRestrictionsFeatureEnabled = true,
                IsAccountRestrictionsSettingEnabled = false,
                IsAccountSettingsSocialNetworksV2Enabled = false,
                IsUiBootstrapModalV2Enabled = true,
                IsI18nBirthdayPickerInAccountSettingsEnabled = true,
                InApp = false,
                MyAccountSecurityModel = new
                {
                    IsEmailSet = true,
                    IsEmailVerified = true,
                    IsTwoStepEnabled = false,
                    ShowSignOutFromAllSessions = true,
                    TwoStepVerificationViewModel = new
                    {
                        UserId = safeUserSession.userId,
                        IsEnabled = false,
                        CodeLength = 6,
                        ValidCodeCharacters = (int?)null
                    }
                },
                ApiProxyDomain = Configuration.BaseUrl,
                AccountSettingsApiDomain = Configuration.BaseUrl,
                AuthDomain = Configuration.BaseUrl,
                IsDisconnectFbSocialSignOnEnabled = true,
                IsDisconnectXboxEnabled = true,
                NotificationSettingsDomain = Configuration.BaseUrl,
                AllowedNotificationSourceTypes = new List<string>
                {
                    "Test",
                    "FriendRequestReceived",
                    "FriendRequestAccepted",
                    "PartyInviteReceived",
                    "PartyMemberJoined",
                    "ChatNewMessage",
                    "PrivateMessageReceived",
                    "UserAddedToPrivateServerWhiteList",
                    "ConversationUniverseChanged",
                    "TeamCreateInvite",
                    "GameUpdate",
                    "DeveloperMetricsAvailable"
                },
                AllowedReceiverDestinationTypes = new List<string>
                {
                    "DesktopPush",
                    "NotificationStream"
                },
                BlacklistedNotificationSourceTypesForMobilePush = new List<string> { },
                MinimumChromeVersionForPushNotifications = 50,
                PushNotificationsEnabledOnFirefox = true,
                LocaleApiDomain = Configuration.BaseUrl,
                HasValidPasswordSet = true,
                IsUpdateEmailApiEndpointEnabled = true,
                FastTrackMember = (string?)null,
                IsFastTrackAccessible = false,
                HasFreeNameChange = false,
                IsAgeDownEnabled = false,
                IsSendVerifyEmailApiEndpointEnabled = true,
                IsPromotionChannelsEndpointEnabled = true,
                ReceiveNewsletter = false,
                SocialNetworksVisibilityPrivacy = 6,
                SocialNetworksVisibilityPrivacyValue = "AllUsers",
                Facebook = (string?)null,
                Twitter = (string?)null,
                YouTube = (string?)null,
                Twitch = (string?)null
            };
        }
        [HttpGetBypass("v2/stream-notifications/unread-count")]
        public dynamic PushNotif()
        {
            return new 
            {
                unreadNotifications = 69,
                statusMessage = string.Empty
            };
        }

        [HttpGetBypass("sponsoredpage/list-json")]
        [HttpGetBypass("mobile-ads/v1/get-ad-details")]
        [HttpGetBypass("incoming-items/counts")]
        public dynamic IncomingItems()
        {
            return new 
            {
                success = true
            };
        }

        [HttpGetBypass("v1.1/game-start-info")]
        public dynamic GameStartInfo(long universeId)
        {
            return new
            {
                gameAvatarType = "PlayerChoice",
                allowCustomAnimations = "True",
                universeAvatarCollisionType = "OuterBox",
                universeAvatarBodyType = "Standard",
                jointPositioningType = "ArtistIntent",
                message = "",
                universeAvatarMinScales = new
                {
                    height = 0.9,
                    width = 0.7,
                    head = 0.95,
                    depth = 0.0,
                    proportion = 0.0,
                    bodyType = 0.0
                },
                universeAvatarMaxScales = new
                {
                    height = 1.05,
                    width = 1.0,
                    head = 1.0,
                    depth = 0.0,
                    proportion = 1.0,
                    bodyType = 1.0
                },
                universeAvatarAssetOverrides = new List<object>(),
                moderationStatus = ""
            };
        }
        [HttpGetBypass("hor")]
        public async Task<IActionResult> HallOfRetards()
        {
            return Content(await System.IO.File.ReadAllTextAsync("hor.txt"), "text/plan");
        }
        [HttpGetBypass("/v1/avatar-fetch")]
        [HttpGetBypass("/v1.1/avatar-fetch")]
        public async Task<MVC.IActionResult> CharacterFetch(long userId)
        {
            List<long> accessoryVersionIds = new List<long>();
            List<long> equippedGearVersionIds = new List<long>();
            string userAgent = Request.Headers["User-Agent"].ToString();
            var wornAssets = await services.avatar.GetWornAssets(userId);
            var avatar = await services.avatar.GetAvatar(userId);
            List<dynamic> emotes = new List<dynamic>
            {
                new
                {
                    assetId = 15610015346,
                    assetName = "Yungblud Happier Jump",
                    position = 1
                },
                new
                {
                    assetId = 14124050904,
                    assetName = "TWICE Like Ooh-Ahh",
                    position = 2
                },
            };

            var assetInfo = await services.assets.MultiGetInfoById(wornAssets);
            dynamic bodyColors = new
            {
                headColorId = avatar.headColorId,
                leftArmColorId = avatar.leftArmColorId,
                leftLegColorId = avatar.leftLegColorId,
                rightArmColorId = avatar.rightArmColorId,
                rightLegColorId = avatar.rightLegColorId,
                torsoColorId = avatar.torsoColorId,

                HeadColor = avatar.headColorId,
                LeftArmColor = avatar.leftArmColorId,
                LeftLegColor = avatar.leftLegColorId,
                RightArmColor = avatar.rightArmColorId,
                RightLegColor = avatar.rightLegColorId,
                TorsoColor = avatar.torsoColorId
            };
            dynamic scales = new { height = 1, Height = 1, width = 1, Width = 1, head = 1, Head = 1, Depth = 1, depth = 1, proportion = 0, Proportion = 0, bodyType = 0, BodyType = 0};
            string AvatarType = (avatar.avatar_type == 2) ? "R15" : "R6";
            foreach (long assetId in wornAssets)
            {
                var catinfo = await services.assets.GetAssetCatalogInfo(assetId);
                if (catinfo.assetType == Type.Gear){
                    equippedGearVersionIds.Add(assetId);
                }
                else{
                    accessoryVersionIds.Add(assetId);
                }
            }
            if (userAgent != "Roblox/Win2020"){
                equippedGearVersionIds = new List<long>();
            }
            var result = new {
                resolvedAvatarType = AvatarType,
                accessoryVersionIds,
                equippedGearVersionIds,
                assetAndAssetTypeIds = assetInfo.Select(c =>
                {
                    return new
                    {
                        assetId = c.id,
                        assetTypeId = (int)c.assetType,
                    };
                }),
                backpackGearVersionIds = equippedGearVersionIds,
                animationAssetIds = new {},
                playerAvatarType = AvatarType,
                scales,
                bodyColorsUrl = $"{Configuration.BaseUrl}/Asset/BodyColors.ashx?userId={userId}",
                bodyColors,
                emotes
            };
            string jsonString = JsonConvert.SerializeObject(result);
            return Content(jsonString, "application/json");
        }
        
        private void CheckServerAuth(string auth)
        {
            if (auth != Configuration.GameServerAuthorization)
            {
                throw new BadRequestException();
            }
        }

        [HttpPostBypass("/gs/activity")]
        public async Task<dynamic> GetGsActivity([Required, MVC.FromBody] ReportActivity request)
        {
            Console.WriteLine(request.authorization);

            CheckServerAuth(request.authorization);
            var result = await services.gameServer.GetLastServerPing(request.serverId);
            return new
            {
                isAlive = result >= DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                updatedAt = result,
            };
        }
        [HttpPostBypass("/gs/ping")]
        public async Task ReportServerActivity([Required, MVC.FromBody] ReportActivity request)
        {
            CheckServerAuth(request.authorization);
            //await services.gameServer.SetServerGSPing(request.serverId, request.ping);
            await services.gameServer.SetServerPing(request.serverId);
        }

        [HttpPostBypass("/gs/delete")]
        public async Task DeleteServer([Required, MVC.FromBody] ReportActivity request)
        {
            CheckServerAuth(request.authorization);
            await services.gameServer.DeleteGameServer(request.serverId);
        }
        //this is for the newer years that dont have a custom monitoring script
        [HttpPostBypass("presence/register-game-presence")]
        public async Task<dynamic> RegisterGamePresence(long visitorId, long placeId, string gameId, string locationType) 
        {
            bool IsRCC = IsRcc();
            if(!IsRCC)
            {
                throw new UnauthorizedAccessException();
            }

            await services.gameServer.OnPlayerJoin(visitorId, placeId, gameId);
            return Ok();
        }

        [HttpPostBypass("presence/register-absence")]
        public async Task RegisterGamePresenceAbsence(long visitorId)
        {
            bool IsRCC = IsRcc();
            if(!IsRCC)
            {
                return;
            }
            string JobId = await services.gameServer.GetJobIdByUserId(visitorId);
            if(JobId == null)
            {
                return;
            }
            long placeId = GameServerService.GetUserPlaceId(visitorId);

            await services.gameServer.OnPlayerLeave(visitorId, placeId, JobId);
        }
        [HttpGetBypass("/device/initialize")]
        [HttpPostBypass("/device/initialize")]
        public ActionResult<dynamic> InitDevice()
        {
            string? appDeviceIdentifier = null;

            var json = new
            {
                browserTrackerId = 1234567890,
                appDeviceIdentifier = appDeviceIdentifier,
            };

            string? jsonString = JsonConvert.SerializeObject(json);
            return Content(jsonString, "application/json");
        }
        [HttpGetBypass("/Game/ClientPresence.ashx")]
        public async Task ClientPresenceAshx(string action, long placeId, long userId, bool IsTeleport)
        {
            bool IsRCC = IsRcc();
            if(!IsRCC)
            {
                return;
            }
            if(action == "disconnect"){
                string JobId = await services.gameServer.GetJobIdByUserId(userId);
                if(JobId == null)
                {
                    return;
                }
                await services.gameServer.OnPlayerLeave(userId, placeId, JobId);
            }
        }
        [HttpPostBypass("/gs/shutdown")]
        public async Task ShutDownServer([Required, MVC.FromBody] ReportActivity request)
        {
            CheckServerAuth(request.authorization);
            await services.gameServer.ShutDownServerAsync(request.serverId);
        }

        [HttpPostBypass("/gs/players/report")]
        public async Task ReportPlayerActivity([Required, MVC.FromBody] ReportPlayerActivity request)
        {
            CheckServerAuth(request.authorization);
            if (request.eventType == "Leave")
            {
                await services.gameServer.OnPlayerLeave(request.userId, request.placeId, request.serverId);
            }
            else if (request.eventType == "Join")
            {
                await Roblox.Metrics.GameMetrics.ReportGameJoinSuccess(request.placeId);
                await services.gameServer.OnPlayerJoin(request.userId, request.placeId, request.serverId);
            }
            else
            {
                throw new Exception("Unexpected type " + request.eventType);
            }
        }

        [HttpPostBypass("/gs/a")]
        public void ReportGS()
        {
            // Doesn't do anything yet. See: services/api/src/controllers/bypass.ts:1473
            return;
        }
        [HttpPostBypass("game/validate-machine")]
        public dynamic ValidateMachineAsync()
        {
            return new
            {
                success = true,
                message = "",
            };
            /*
            HWID hwid = new HWID();
            long userId = safeUserSession.userId;
            bool isBanned = false; 
            string macAddress = null; 

            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string rawBody = await reader.ReadToEndAsync();

                string[] macAddresses = rawBody.Split('&');
                List<string> processedMacAddresses = new List<string>();
                foreach (string macAddressString in macAddresses)
                {
                    string[] parts = macAddressString.Split('=');
                    if (parts.Length == 2)
                    {
                        macAddress = parts[1];
                        isBanned = await hwid.CheckHWID(userId, macAddress);
                        if (!isBanned) 
                        {
                            break;
                        }                        
                    }
                }
            }

            if (macAddress == null)
            {
                return new
                {
                    success = false,
                    message = "Invalid Data",
                };
            }

            return new
            {
                success = isBanned,
                message = "",
            };
            */
        }
        [HttpGetBypass("/v1/user/currency")]
        [HttpGetBypass("/my/balance")]
        public async Task<ActionResult<dynamic>> MyBalance()
        {

            var bal = await services.economy.GetUserRobux(safeUserSession.userId);
            var json = new
            {
                robux = bal
            };

            string? jsonString = JsonConvert.SerializeObject(json);
            return Content(jsonString, "application/json");
        }
        [HttpGetBypass("Users/ListStaff.ashx")]
        public async Task<dynamic> GetStaffList()
        {
            if(!IsRcc()) return Redirect("/404");
            return (await StaffFilter.GetStaff()).Where(c => c != 12);
        }

        [HttpGetBypass("GenerateOtpSecret")]
        public async Task<dynamic> GenerateOtpSecret()
        {
            var totpInfo = await services.users.GetOrSetTotp(safeUserSession.userId);
            return totpInfo.secret;
        }

        [HttpGetBypass("GenereateOtpQrCode")]
        public IActionResult GenerateOtpQrCode(string secret)
        {
            var otpQrCode = services.users.GetOtpQrCode(safeUserSession.userId, secret);
            return File(otpQrCode, "image/png");
        }

        [HttpGetBypass("Users/GetBanStatus.ashx")]
        public async Task<IEnumerable<dynamic>> MultiGetBanStatus(string userIds)
        {

            var ids = userIds.Split(",").Select(long.Parse).Distinct();
            var result = new List<dynamic>();
#if DEBUG
            return ids.Select(c => new
            {
                userId = c,
                isBanned = false,
            });
#else
            var multiGetResult = await services.users.MultiGetAccountStatus(ids);
            foreach (var user in multiGetResult)
            {
                result.Add(new
                {
                    userId = user.userId,
                    isBanned = user.accountStatus != AccountStatus.Ok,
                });
            }

            return result;
#endif
        }

        [HttpGetBypass("Asset/BodyColors.ashx")]
        public async Task<string> GetBodyColors(long userId)
        {
            var colors = await services.avatar.GetAvatar(userId);

            var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

            var robloxRoot = new XElement("roblox",
                new XAttribute(XNamespace.Xmlns + "xmime", "http://www.w3.org/2005/05/xmlmime"),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(xsi + "noNamespaceSchemaLocation", "http://www.roblox.com/roblox.xsd"),
                new XAttribute("version", 4)
            );
            robloxRoot.Add(new XElement("External", "null"));
            robloxRoot.Add(new XElement("External", "nil"));
            var items = new XElement("Item", new XAttribute("class", "BodyColors"));
            var properties = new XElement("Properties");
            // set colors
            properties.Add(new XElement("int", new XAttribute("name", "HeadColor"), colors.headColorId.ToString()));
            properties.Add(new XElement("int", new XAttribute("name", "LeftArmColor"), colors.leftArmColorId.ToString()));
            properties.Add(new XElement("int", new XAttribute("name", "LeftLegColor"), colors.leftLegColorId.ToString()));
            properties.Add(new XElement("string", new XAttribute("name", "Name"), "Body Colors"));
            properties.Add(new XElement("int", new XAttribute("name", "RightArmColor"), colors.rightArmColorId.ToString()));
            properties.Add(new XElement("int", new XAttribute("name", "RightLegColor"), colors.rightLegColorId.ToString()));
            properties.Add(new XElement("int", new XAttribute("name", "TorsoColor"), colors.torsoColorId.ToString()));
            properties.Add(new XElement("bool", new XAttribute("name", "archivable"), "true"));
            // add
            items.Add(properties);
            robloxRoot.Add(items);
            // return as string
            return new XDocument(robloxRoot).ToString();
        }


        [HttpPostBypass("Game/PlaceVisit.ashx")]

        [HttpGetBypass("Game/PlaceVisit.ashx")]
        public dynamic PlaceVisit()
        {
            return Ok();
        }
        [HttpGetBypass("rcc/killallservers")]
        public async Task<dynamic> ShutdownServersForPlace(long placeId)
        {
            string jobId;
            var serverjobs = await services.gameServer.GetGameServersForPlace(placeId);
            bool canManagePlace = await services.assets.CanUserModifyItem(placeId, safeUserSession.userId);
            if (canManagePlace)
            {
                foreach (var jobs in serverjobs)
                {
                    jobId = jobs.id.ToString(); 
                    await services.gameServer.ShutDownServerAsync(jobId);
                }
                return "OK!";
            }
            else{
                return "Unauthorized";
            }
        }
        [HttpGetBypass("rcc/kickplayer")]
        public async Task<dynamic> KickPlayerAsync(long userId)
        {
            bool isOwner = userSession != null && StaffFilter.IsOwner(safeUserSession.userId);

            if (safeUserSession.userId == userId)
            {
                return "You can't kick yourself!";
            }
            if (isOwner)
            {
                await services.gameServer.KickPlayer(userId);
                return $"Kicked player {userId}";
            }
            else
            {
                return "not the owner";
            }
        }

        [HttpGetBypass("/Game/ChatFilter.ashx")]
        public string RCC_GetChatFilter()
        {
            return "True";
        }
        [HttpPostBypass("moderation/filtertext/")]
        public dynamic GetModerationText()
        {
            var text = services.filter.FilterText(HttpContext.Request.Form["text"].ToString());
            return new
            {
                success = true,
                data = new 
                {
                    white = text,
                    black = text
                }
            };
        }
        [HttpPostBypass("moderation/v2/filtertext/")]
        public dynamic GetModerationTextV2()
        {
            var text = services.filter.FilterText(HttpContext.Request.Form["text"].ToString());
            var json = new
            {
                success = true,
                data = new
                {
                    AgeUnder13 = text,
                    Age13OrOver = text,
                }
            };
            string jsonString = JsonConvert.SerializeObject(json);
            return Content(jsonString, "application/json");
        }

        private void ValidateBotAuthorization()
        {
#if DEBUG == false
	        if (Request.Headers["bot-auth"].ToString() != Roblox.Configuration.BotAuthorization)
	        {
		        throw new Exception("Intern al");
	        }
#endif
        }

        [HttpGetBypass("botapi/migrate-alltypes")]
        public async Task<dynamic> MigrateAllItemsBot([Required, MVC.FromQuery] string url)
        {
            ValidateBotAuthorization();
            return await MigrateItem.MigrateItemFromRoblox(url, false, null, new List<Type>()
            {
                Type.Image,
                Type.Audio,
                Type.Mesh,
                Type.Lua,
                Type.Model,
                Type.Decal,
                Type.Animation,
                Type.SolidModel,
                Type.MeshPart,
                Type.GamePass,
                Type.ClimbAnimation,
                Type.DeathAnimation,
                Type.FallAnimation,
                Type.IdleAnimation,
                Type.JumpAnimation,
                Type.RunAnimation,
                Type.SwimAnimation,
                Type.WalkAnimation,
                Type.PoseAnimation,
            }, default, false);
        }

        [HttpGetBypass("botapi/migrate-clothing")]
        public async Task<dynamic> MigrateClothingBot([Required] string assetId)
        {
            ValidateBotAuthorization();
            return await MigrateItem.MigrateItemFromRoblox(assetId, true, 5, new List<Models.Assets.Type>() { Models.Assets.Type.TeeShirt, Models.Assets.Type.Shirt, Models.Assets.Type.Pants });
        }

        [HttpGetBypass("BuildersClub/Upgrade.ashx")]
        public MVC.IActionResult UpgradeNow()
        {
            return new MVC.RedirectResult("/internal/membership");
        }
        [HttpGetBypass("game/players/{userId}")]
        public MVC.ActionResult<dynamic> ChatWhiteList(long userId)
        {
            string mode;
            bool IsOwner = StaffFilter.IsOwner(userId);
            if (StaffFilter.IsOwner(userId)){
                mode = "whitelist";
            }
            else{
                mode = "blacklist";
            }
            return new 
            {
                ChatFilter = mode,
            };
        }

        [HttpGetBypass("GetAllowedMD5Hashes")]
        public MVC.ActionResult<dynamic> AllowedMD5Hashes()
        {
            if (!IsRcc())
                throw new RobloxException(400, 0, "BadRequest");
            List<string> allowedList = new List<string>()
            {
                "a9912debcb6347c402e4139f452d4fd2", //2015M Prod
                "d902c5a3a4a33954bc6fbd0daa485966", //2016E Prod
                "fc5f43ec839bbbffcb26c48846b3c865", //2017L RAGELoader Debug
                "81b26cbf32171b1691d4b5d135e0ef84", //2017L Prod
                "8c5aecb7811acbb582f06f2a81b958f4", //2018L Prod
                "4022369076d608d1a99b7b3d250e4de5", //2018L RAGELoader Debug
                "9d7975454cee0e948e35cdc1fb55f92a", //2019E Prod
                "9cdc73fd9b24c974f5a0dde411dcd38f", //2020L Prod
                "a58f725954fe5b2ed30b0778f932d249", //2021M Prod
                "1fd6e7becff68acc140b2db17e24c86e", //2021M June 6
            };

            return new { data = allowedList };
        }

        [HttpGetBypass("GetAllowedSecurityKeys")]
        public MVC.ActionResult<dynamic> AllowedSecurity()
        {
            return true;
        }
        [HttpGetBypass("GetAllowedSecurityVersions")]
        public MVC.ActionResult<dynamic> AllowedSecurityVersions()
        {
            if (!IsRcc())
                throw new RobloxException(400, 0, "BadRequest");
            List<string> allowedList = new List<string>()
            {  
                "0.206.0pcplayer",
                "0.235.0pcplayer",
                "0.314.0pcplayer",
                "0.376.0pcplayer",
                "0.355.0pcplayer",
                "2.355.0iosapp",
                "0.450.0pcplayer",
                "0.463.0pcplayer"
            };
            var jsonString = JsonConvert.SerializeObject(allowedList);
            return new { data = jsonString };
        }
        private static int pendingAssetUploads { get; set; } = 0;
        private static readonly Mutex pendingAssetUploadsMux = new();

        [HttpPostBypass("Data/Upload.ashx")]
        public async Task<dynamic> Upload(long assetId)
        {
            var info = await services.assets.GetAssetCatalogInfo(assetId);
            var canUpload = await services.assets.CanUserModifyItem(info.id, safeUserSession.userId);

            if (info.assetType != Models.Assets.Type.Place)
            {
                canUpload = false;
            }

            if (canUpload == false)
                throw new RobloxException(403, 0, "Unauthorized");

            lock (pendingAssetUploadsMux)
            {
                if (pendingAssetUploads >= 2)
                    throw new RobloxException(429, 0, "TooManyRequests");
                pendingAssetUploads++;
            }
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await HttpContext.Request.Body.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using (Stream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        await gzipStream.CopyToAsync(decompressedStream);
                        decompressedStream.Position = 0;

                        bool assetValidated = await services.assets.ValidateAssetFile(decompressedStream, Type.Place);
                        if (!assetValidated)
                        {
                            throw new RobloxException(400, 0, "The asset file doesn't look correct. Please try again.");
                        }
                        
                        decompressedStream.Position = 0;

                        await services.assets.CreateAssetVersion(assetId, safeUserSession.userId, decompressedStream);
                        await services.assets.RenderAssetAsync(assetId, info.assetType);
                    }
                }
            }
            finally
            {
                lock (pendingAssetUploadsMux)
                {
                    pendingAssetUploads--;
                }
                
            }
            return new
            {
                success = true,
            };
        }
        private async Task<bool> AssetValidationV2(Stream stream)
        {
            byte[] buffer = new byte[7]; 
            await stream.ReadAsync(buffer, 0, buffer.Length);
            string startOfFile = Encoding.UTF8.GetString(buffer);
            return startOfFile == "<roblox";
        }
        [HttpPostBypass("universes/{universeId:long}/enablecloudedit")]
        public async Task<OkObjectResult> EnableCloudEdit(long universeId)
        {
            await services.games.EnableCloudEdit(universeId);
            return Ok(new { });
        }

        [HttpGetBypass("universes/{universeId:long}/cloudeditenabled")]
        public dynamic IsCloudEditEnabled(long universeId)
        {
            return new 
            {
                enabled = false,
            };
        }
        /*
        [HttpGetBypass("universes/get-aliases")]
        public async Task<dynamic> GetAliases(long universeId)
        {
            
            return new
            {
                FinalPage = true,
                Aliases = new[]
                {
                    new 
                    {
                        Name = "Test",
                        Type = (int)Models.Assets.Type.Hat,
                        TargetId = 164,
                        Asset = new
                        {
                            Id = 164,
                            TypeId = (int)Models.Assets.Type.Hat,
                            Name = "Name",
                            Description = "Name",
                            CreatorType = (int)CreatorType.User,
                            CreatorTargetId = 1,
                            Created = "2017-03-31T12:16:46.547",
                            Updated = "2017-03-31T12:16:46.547",   
                        },
                        Version = 0,
                    }
                },
                PageSize = 50
            };   
        }
        */
        [HttpGetBypass("v1/user/{userId:long}/is-admin-developer-console-enabled")]
        public async Task<dynamic> NewCanManage(long userId)
        {
            long placeId = long.Parse(Request.Headers["roblox-place-id"].ToString());
            bool canManagePlace = await services.assets.CanUserModifyItem(placeId, userId);
            bool isOwner =  StaffFilter.IsOwner(userId);
            return new 
            {
                isAdminDeveloperConsoleEnabled = (canManagePlace || isOwner)
            };
        }
        [HttpGetBypass("universes/get-universe-places")]
        public async Task<dynamic> GetPlaces(long universeId)
        {
            var place = await services.games.GetRootPlaceId(universeId);
            var placeInfo = await services.assets.GetAssetCatalogInfo(place);
            return new
            {
                FinalPage = true,
                RootPlace = place,
                Places = new
                {
                    PlaceId = place,
                    Name = placeInfo.name,
                },
                PageSize = 50
            };
        }

        [HttpGetBypass("game/validate-place-join")]
        [HttpPostBypass("universes/validate-place-join")]
        [HttpGetBypass("universes/validate-place-join")]
        public MVC.ActionResult<dynamic> ValidateJoin()
        {
            return "true";
        }

        [HttpGetBypass("v2/get-rollout-settings")]
        public dynamic ChatRollout(string featureNames)
        {
            dynamic rollOut = new
            {
                rolloutFeatures = new[]
                {
                    new
                    {
                        featureName = featureNames,
                        isRolloutEnabled = true
                    }
                }
            };

            string jsonString = JsonConvert.SerializeObject(rollOut);
            return Content(jsonString, "application/json");
        }


        [HttpGetBypass("abusereport/UserProfile"), HttpGetBypass("abusereport/asset"), HttpGetBypass("abusereport/user"), HttpGetBypass("abusereport/users")]
        public MVC.IActionResult ReportAbuseRedirect()
        {
            return new MVC.RedirectResult("/internal/report-abuse");
        }

        [HttpGetBypass("/info/blog")]
        public MVC.IActionResult RedirectToUpdates()
        {
            return new MVC.RedirectResult("/internal/updates");
        }

        [HttpGetBypass("/my/economy-status")]
        public dynamic GetEconomyStatus()
        {
            return new
            {
                isMarketplaceEnabled = true,
                isMarketplaceEnabledForAuthenticatedUser = true,
                isMarketplaceEnabledForUser = true,
                isMarketplaceEnabledForGroup = true,
            };
        }

        [HttpGetBypass("/currency/balance")]
        public async Task<dynamic> GetBalance()
        {
            return await services.economy.GetBalance(CreatorType.User, safeUserSession.userId);
        }

        [HttpGetBypass("/ownership/hasasset")]
        public async Task<string> DoesOwnAsset(long userId, long assetId)
        {
            return (await services.users.GetUserAssets(userId, assetId)).Any() ? "true" : "false";
        }

        [HttpGetBypass("sign-out/v1")]
        [HttpGetBypass("game/logout.aspx")]
        public dynamic Logout()
        {
            using var sessCache = Roblox.Services.ServiceProvider.GetOrCreate<UserSessionsCache>();
            sessCache.Remove(safeUserSession.sessionId);
            HttpContext.Response.Cookies.Delete(Middleware.SessionMiddleware.CookieName);
            return Ok();
        }

        [HttpGetBypass("user/follow")]
        [HttpPost("user/follow")]
        public async Task<dynamic> FollowUser(long followedUserId)
        {
            FeatureFlags.FeatureCheck(FeatureFlag.FollowingEnabled);
            if (followedUserId == safeUserSession.userId)
                throw new BadRequestException();
            await services.friends.FollowerUser(safeUserSession.userId, followedUserId);

            return new
            {
                success = true,
                isCaptchaRequired = false,
            };
        }
        [HttpGetBypass("users/get-by-username")]
        public async Task<dynamic> GetByUsername(string username)
        {
            var userInfo = await services.users.GetUserByName(username);
            var onlineStatus = (await services.users.MultiGetPresence(new[] {userInfo.userId})).First();
            return new 
            {
                Id = userInfo.userId,
                Username = username,
                AvatarUri = "null",
                AvatarFinal = false,
                IsOnline = onlineStatus.userPresenceType,
            };
        }
        [HttpGetBypass("users/account-info")]
        [HttpPostBypass("users/account-info")]
        public async Task<dynamic> accountInfo()
        {

            var roles = new string[] { };
            if (userSession == null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return new
                {
                    success = false,
                    message = "Unauthorized"
                };
            }
            var userBalance = await services.economy.GetUserBalance(userSession.userId);
            var jsonData = new
            {
                UserId =  userSession.userId,
                Username = userSession.username,
                DisplayName = userSession.username,
                HasPasswordSet = true,
                Email = "ProjectX@projex.zip",
                MembershipType = 3,
                RobuxBalance = userBalance.robux,
                AgeBracket = 0,
                Roles = roles,
                EmailNotificationEnabled = false,
                PasswordNotifcationEnabled = false,
            };
            string jsonString = JsonConvert.SerializeObject(jsonData);
            return Content(jsonString, "application/json");
        }
        [HttpPostBypass("user/following-exists")]
        [HttpGetBypass("user/following-exists")]
        public async Task<dynamic> FollowingExists(long userId, long followerUserId)
        {
            var result = new List<dynamic>();
                if (userSession is null)
                {
                    result.Add(new
                    {
                        isFollowing = false,
                        userId,
                    });
                }
                
                var isFollowing = await services.friends.IsOneFollowingTwo(safeUserSession.userId, followerUserId);
                result.Add(new
                {
                    isFollowing,
                    userId,
                });
            
            return new
            {
                followings = result,
            };
        }
        [HttpPost("user/unfollow")]
        public async Task DeleteFollowing(long followedUserId)
        {
            FeatureFlags.FeatureCheck(FeatureFlag.FollowingEnabled);
            await services.friends.DeleteFollowing(safeUserSession.userId, followedUserId);
        }
        [HttpPost("user/decline-friend-request")]
        public async Task DeclineFriendRequest(long requesterUserId)
        {
            FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
            await services.friends.DeclineFriendRequest(safeUserSession.userId, requesterUserId);
        }
        [HttpGetBypass("user/request-friendship")]
        [HttpPostBypass("user/request-friendship")]
        public async Task<dynamic> RequestFriendship(long recipientUserId)
        {
            FeatureFlags.FeatureCheck(FeatureFlag.FriendingEnabled);
            if (safeUserSession.userId == recipientUserId)
                throw new BadRequestException(7, "The user cannot be friends with itself");
            await services.friends.RequestFriendship(safeUserSession.userId, recipientUserId);
            
            return new
            {
                success = true,
                isCaptchaRequired = false,
            };
        }
        [HttpGetBypass("user/get-friendship-count")]
        public async Task<dynamic> GetFriendsAmount(long? userId)
        {
            if(userId == null)
            {
                userId = safeUserSession.userId;
            }
            int amountFriends = await services.friends.CountFriends((long)userId);
            return new 
            {
                success = true,
                message = "Success",
                count = amountFriends
            };
        }
        [HttpGetBypass("/asset/getyear")]
        public async Task<dynamic> GetPlaceYear(long placeId)
        {
            long year = await services.games.GetYear(placeId);
            return year;
        }
        [HttpPostBypass("game/load-place-info")]
        public async Task<dynamic> LoadPlaceInfo()
        {
            var placeId = Request.Headers["roblox-place-id"];
            long.TryParse(placeId, out long assetId);
            var details = await services.assets.GetAssetCatalogInfo(assetId);
            var jsonData = new
            {
                CreatorId =  details.creatorTargetId,
                CreatorType = "User",
                PlaceVersion = details.id,
                GameId = assetId,
                IsRobloxPlace = details.creatorTargetId == 1
            };
            string jsonString = JsonConvert.SerializeObject(jsonData);
            return Content(jsonString, "application/json");
        }


        [HttpGetBypass("studio/e.png")]
        public string StudioEpng()
        {
            return "1";
        }
        [HttpGetBypass("GetCurrentClientVersionUpload")]
        public ActionResult<dynamic> ReturnCurrentClientVersion(string binaryType)
        {
            switch (binaryType)
            {

                case "MacPlayer":
                    return @"""version-z1425cxd4e0c4a2""";
                case "MacStudio":
                    return @"""version-z1425cxd4e0c4a2""";
                default:
                    return @"""version-d23df1d1a8d546ee""";
            }
        }
        /*
        [HttpGetBypass("v1/Close")]
        [HttpPostBypass("V1/Close")]
        public async Task<dynamic> CloseGSNew(string gameId)
        {

            bool IsRCC = IsRcc();
            if(!IsRCC)
            {
                return "Not RCC";
            }

            try
            {
                await services.gameServer.ShutDownServerAsync(gameId);
                return "OK!";
            }
            catch (Exception)
            {
                // lets just delete the gameserver if we couldnt close the gameserver 
                await services.gameServer.DeleteGameServer(gameId);
                return "Catch an error";
            }
        }
        */
        [HttpPostBypass("v2/CreateOrUpdate")]        
        [HttpGetBypass("v2/CreateOrUpdate")]
        [HttpGetBypass("v1/CreateOrUpdate")]
        [HttpPostBypass("v1/CreateOrUpdate")]        
        public async Task<dynamic> GetOrCreate(string gameId, decimal ping)
        {
            bool IsRCC = IsRcc();
            int roundPing = (int)Math.Round(ping, 0);            
            if(!IsRCC)
            {
                return "Not RCC";
            }
            
            await services.gameServer.SetServerGSPing(gameId, roundPing);   
            return "OK!";             
            
        }        
        [HttpPostBypass("v1.0/Refresh")]
        [HttpPostBypass("v2.0/Refresh")]
        [HttpGetBypass("v1.0/Refresh")]
        [HttpGetBypass("v2.0/Refresh")]
        public async Task<dynamic> RefreshGameInstance(string gameId, long clientCount, Decimal gameTime)
        {
            bool IsRCC = IsRcc();
            if (!IsRCC){
                return "Not RCC";
            }

            if (clientCount == 0 && gameTime > 50)
            {
                try
                {
                    await services.gameServer.ShutDownServerAsync(gameId);
                    return "OK!";
                }
                catch (Exception)
                {
                    await services.gameServer.DeleteGameServer(gameId);
                    return "OK!";
                }
            }
            else{
                await services.gameServer.SetServerPing(gameId);
                return "OK!";
            }
        }
        // just a test move to webcontroller later on
        [HttpPostBypass("apisite/develop/v1/assets/upload-gameicon")]
        public async Task<dynamic> UploadGameIcon(long placeId, [Required, FromForm] IFormFile file)
        {
            await services.assets.ValidatePermissions(placeId, safeUserSession.userId);
            var details = await services.assets.GetAssetCatalogInfo(placeId);
            if (details.assetType != Models.Assets.Type.Place) {
                throw new BadRequestException(1, "Cannot upload a game icon for a non place");
            }
            
            await services.assets.CreateGameIcon(placeId, file.OpenReadStream());
            return Ok();
        }
        [HttpPostBypass("/v1.0/SequenceStatistics/AddToSequence")]
        [HttpPostBypass("/v1.1/Counters/Increment")]
        [HttpPostBypass("/v1.0/SequenceStatistics/BatchAddToSequencesV2")]
        [HttpPostBypass("v1.0/MultiIncrement")]
        [HttpPostBypass("/game/report-stats")]
        [HttpGetBypass("usercheck/show-tos")]
        [HttpGetBypass("/v1.1/Counters/Increment")]
        [HttpGetBypass("notifications/signalr/negotiate")]
        [HttpGetBypass("notifications/negotiate")]
        [HttpPostBypass("v1.1/Counters/BatchIncrement")]
        [HttpGetBypass("v1.1/Counters/BatchIncrement")]
        public MVC.OkResult TelemetryFunctions()
        {
            return Ok();
        }
          
#if DEBUG
        [HttpGetBypass("integration-test/create-account-and-set-cookie")]
        public async Task<string> CreateAccountAndSetCookie()
        {
            var result = await services.users.CreateUser("ROBLOX", "ROBLOX", Gender.Male);
            await services.users.InsertOrUpdateMembership(result.userId, MembershipType.BuildersClub);
            var id = await services.users.CreateApplication(new CreateUserApplicationRequest()
            {
                about = "ROBLOX",
                socialPresence = "",
                isVerified = true,
                verifiedUrl = Configuration.BaseUrl,
                verificationPhrase = "Integration test",
                verifiedId = "1",
            });
            var joinId = await services.users.ProcessApplication(id, 1, UserApplicationStatus.Approved);
            await services.users.SetApplicationUserIdByJoinId(joinId, result.userId);
            
            var sess = await services.users.CreateSession(result.userId);
            var sessionCookie = Roblox.Website.Middleware.SessionMiddleware.CreateJwt(new Middleware.JwtEntry()
            {
                sessionId = sess,
                createdAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
            });
            Response.Cookies.Append(SessionMiddleware.CookieName, sessionCookie, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.Now.AddDays(1),
                Path = "/",
            });
            return "Created user " + "ROBLOX" + "...\nOK";
        }
#endif
    }
}

