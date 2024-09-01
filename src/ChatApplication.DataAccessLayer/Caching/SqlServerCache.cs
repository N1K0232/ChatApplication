using System.Text;
using System.Text.Json;
using ChatApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ChatApplication.DataAccessLayer.Caching;

public class SqlServerCache(IDistributedCache cache, ILogger<SqlServerCache> logger) : ISqlServerCache
{
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("deleting entity");
        await cache.RemoveAsync(id.ToString(), cancellationToken);
    }

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        logger.LogInformation("getting entity from cache");
        var content = await cache.GetAsync(id.ToString(), cancellationToken);

        if (content is null)
        {
            logger.LogError("no entity found");
            return null;
        }

        var json = Encoding.UTF8.GetString(content);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        logger.LogInformation("inserting entity into cache");

        var json = JsonSerializer.Serialize(entity);
        var bytes = Encoding.UTF8.GetBytes(json);
        await cache.SetAsync(entity.Id.ToString(), bytes, cancellationToken);
    }

    public async Task RefreshAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("updating cache data");
        await cache.RefreshAsync(id.ToString(), cancellationToken);
    }
}