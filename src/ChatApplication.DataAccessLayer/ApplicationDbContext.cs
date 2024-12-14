using System.Reflection;
using ChatApplication.Authentication;
using ChatApplication.DataAccessLayer.Entities.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatApplication.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext
{
    private readonly ILogger<ApplicationDbContext> logger;
    private CancellationTokenSource tokenSource;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger) : base(options)
    {
        this.logger = logger;
        tokenSource = new CancellationTokenSource();
    }

    public Task DeleteAsync<T>(T entity) where T : BaseEntity
    {
        logger.LogInformation("preparing the entity to be deleted");
        Set<T>().Remove(entity);

        return Task.CompletedTask;
    }

    public async ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        logger.LogInformation("search for the entity giving its id");
        var entity = await Set<T>().FindAsync([id], tokenSource.Token);

        return entity;
    }

    public IQueryable<T> GetData<T>(bool trackingChanges = false) where T : BaseEntity
    {
        var set = Set<T>();
        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public async Task InsertAsync<T>(T entity) where T : BaseEntity
    {
        logger.LogInformation("preparing the entity to be added in the database");
        await Set<T>().AddAsync(entity, tokenSource.Token);
    }

    public async Task SaveAsync()
    {
        var affectedRows = await SaveChangesAsync(true, tokenSource.Token);
        logger.LogInformation("Rows updates: {affectedRows}", affectedRows);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(builder);
    }
}