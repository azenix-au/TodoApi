﻿using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Todo.Web.Server;

public static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes, string todoUrl, string prefix)
    {
        // The todo API translates the authentication cookie between the browser the BFF into an 
        // access token that is sent to the todo API. We're using YARP to forward the request.

        var group = routes.MapGroup(prefix);

        group.RequireAuthorization();

        var transformBuilder = routes.ServiceProvider.GetRequiredService<ITransformBuilder>();
        var transform = transformBuilder.Create(b =>
        {
            b.AddRequestTransform(async c =>
            {
                var accessToken = await c.HttpContext.GetTokenAsync(TokenNames.AccessToken);

                c.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
            });
        });

        group.MapForwarder("{*path}", todoUrl, new ForwarderRequestConfig(), transform);

        return group;
    }
}
