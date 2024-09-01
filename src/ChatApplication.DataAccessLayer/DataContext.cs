using ChatApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatApplication.DataAccessLayer;

public class DataContext : DbContext, IDataContext
{
    private readonly ILogger<DataContext> logger;

    public DataContext(DbContextOptions<DataContext> options, ILogger<DataContext> logger) : base(options)
    {
        this.logger = logger;
    }

    public Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async Task InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await Set<T>().AddAsync(entity, cancellationToken);
    }

    public IQueryable<T> GetData<T>(bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>();
        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id], cancellationToken);
        return entity;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var affectedRows = await SaveChangesAsync(true, cancellationToken);
        logger.LogInformation("saved {affectedRows} rows in the database", affectedRows);
    }
}