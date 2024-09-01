using ChatApplication.DataAccessLayer.Entities.Common;

namespace ChatApplication.DataAccessLayer.Caching;

public interface ISqlServerCache
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task RefreshAsync(Guid id, CancellationToken cancellationToken = default);
}