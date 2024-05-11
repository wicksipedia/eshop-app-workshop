using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static TOptions GetOptions<TOptions>(this IHost host)
        where TOptions : class, new()
    {
        return host.Services.GetRequiredService<IOptions<TOptions>>().Value;
    }
}
