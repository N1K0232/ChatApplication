using ChatApplication.DataAccessLayer.Entities.Common;

namespace ChatApplication.DataAccessLayer;

public interface IApplicationDbContext
{
    Task DeleteAsync<T>(T entity) where T : BaseEntity;

    Task InsertAsync<T>(T entity) where T : BaseEntity;

    ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity;

    IQueryable<T> GetData<T>(bool trackingChanges = false) where T : BaseEntity;

    Task SaveAsync();
}