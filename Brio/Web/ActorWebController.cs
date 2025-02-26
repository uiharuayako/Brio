﻿using Brio.Game.Actor;
using Brio.Game.Actor.Extensions;
using Brio.Game.Core;
using Brio.Game.GPose;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System.Threading.Tasks;

namespace Brio.Web;

public class ActorWebController : WebApiController
{
    [Route(HttpVerbs.Post, "/redraw")]
    public async Task<string> RedrawActor([JsonData] RedrawRequest data)
    {
        try
        {
            var result = await Dalamud.Framework.RunUntilSatisfied(
                () =>
                {
                    var gameObject = Dalamud.ObjectTable[data.ObjectIndex];
                    if(gameObject == null)
                        return false;

                    return ActorRedrawService.Instance.CanRedraw(gameObject);
                },
                (success) =>
                {
                    if(success)
                    {
                        var actor = Dalamud.ObjectTable[data.ObjectIndex];
                        if(actor != null)
                            return ActorRedrawService.Instance.Redraw(actor, data.RedrawType ?? RedrawType.All);
                    }
                    return RedrawResult.Failed;

                },
                30);

            return result.ToString();
        }
        catch
        {
            HttpContext.Response.StatusCode = 500;
            return RedrawResult.Failed.ToString();
        }
    }

    [Route(HttpVerbs.Post, "/spawn")]
    public async Task<int> SpawnActor([JsonData] SpawnRequest data)
    {
        try
        {
            return await Dalamud.Framework.RunOnFrameworkThread(() =>
            {
                if(!GPoseService.Instance.IsInGPose)
                    return -1;

                var actorId = ActorSpawnService.Instance.Spawn(data.SpawnOptions);
                if(actorId == null)
                    return -1;

                return (int)actorId;
            });
        }
        catch
        {
            HttpContext.Response.StatusCode = 500;
            return -1;
        }
    }

    [Route(HttpVerbs.Post, "/despawn")]
    public async Task<bool> DespawnActor([JsonData] DespawnRequest data)
    {
        try
        {
            var result = await Dalamud.Framework.RunUntilSatisfied(
                () =>
                {
                    var gameObject = Dalamud.ObjectTable[data.ObjectIndex];
                    if(gameObject == null)
                        return false;

                    return ActorRedrawService.Instance.CanRedraw(gameObject);
                },
                (success) =>
                {
                    if(success)
                    {
                        var actor = Dalamud.ObjectTable[data.ObjectIndex];
                        if(actor != null && actor.IsGPoseActor())
                        {
                            ActorSpawnService.Instance.DestroyObject(actor);
                            return true;
                        }
                    }
                    return false;

                },
                30);

            return result;
        }
        catch
        {
            HttpContext.Response.StatusCode = 500;
            return false;
        }
    }
}

public class RedrawRequest
{
    public int ObjectIndex { get; set; }
    public RedrawType? RedrawType { get; set; }
}


public class DespawnRequest
{
    public int ObjectIndex { get; set; }
}

public class SpawnRequest
{
    public SpawnOptions SpawnOptions { get; set; }
}
