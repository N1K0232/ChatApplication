using ChatApplication.DataAccessLayer.Entities.Common;

namespace ChatApplication.DataAccessLayer;

public interface IDataContext
{
    Task DeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task DeleteAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task InsertAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;

    IQueryable<T> GetData<T>(bool trackingChanges = false) where T : BaseEntity;

    Task SaveAsync(CancellationToken cancellationToken = default);
}