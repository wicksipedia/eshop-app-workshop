using System.Security.Claims;

namespace Grpc.Core;

internal static class GrpcExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.GetUserId();

    public static string? GetUserName(this ServerCallContext context) => context.GetHttpContext().User.GetUserName();
}

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value;

    public static string? GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirst(x => x.Type == "name")?.Value;
}
