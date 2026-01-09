using System.Collections.Concurrent;

namespace TradeWeb.API.Infrastracture;

public sealed class MissingMappingTracker
{
    private readonly ConcurrentDictionary<int, byte> _seen = new();

    public bool ShouldLog(int productId) => _seen.TryAdd(productId, 0);
}