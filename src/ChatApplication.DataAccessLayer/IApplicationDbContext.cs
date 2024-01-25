using ChatApplication.DataAccessLayer.Entities.Common;

namespace ChatApplication.DataAccessLayer;

public interface IApplicationDbContext
{
    void Delete<T>(T entity) where T : BaseEntity;

    void Delete<T>(IEnumerable<T> entities) where T : BaseEntity;

    ValueTask<T> GetAsync<T>(params object[] keyValues) where T : BaseEntity;

    IQueryable<T> Get<T>(bool trackingChanges = false) where T : BaseEntity;

    void Insert<T>(T entity) where T : BaseEntity;

    Task<int> SaveAsync();

    Task ExecuteTransactionAsync(Func<Task> action);
}