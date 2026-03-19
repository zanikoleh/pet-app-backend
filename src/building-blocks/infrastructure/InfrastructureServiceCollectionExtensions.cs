using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server database context to the service collection.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>The DbContextOptionsBuilder for additional configuration.</returns>
    public static DbContextOptionsBuilder AddSqlServerDatabase<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : ApplicationDbContextBase
    {
        return services
            .AddDbContext<TDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions
                        .MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name)
                        .CommandTimeout(30)
                        .EnableRetryOnFailure(2, TimeSpan.FromSeconds(5), null)))
            .LastServiceCollection()
            .AddDbContext<TDbContext>(options =>
                options.UseSqlServer(connectionString))
            .LastServiceCollection()
            .GetService<DbContextOptionsBuilder>() ?? throw new InvalidOperationException();
    }

    private static IServiceCollection LastServiceCollection(this IServiceCollection services)
    {
        return services;
    }
}

/// <summary>
/// Unit of Work pattern implementation for managing transactions across repositories.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    ApplicationDbContextBase DbContext { get; }

    /// <summary>
    /// Commits all changes to the database.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all uncommitted changes.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Base implementation of Unit of Work pattern.
/// </summary>
public abstract class UnitOfWorkBase : IUnitOfWork
{
    protected readonly ApplicationDbContextBase DbContext;
    protected readonly ILogger<UnitOfWorkBase> Logger;

    protected UnitOfWorkBase(
        ApplicationDbContextBase dbContext,
        ILogger<UnitOfWorkBase> logger)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ApplicationDbContextBase Context => DbContext;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error committing changes");
            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DbContext.Database.RollbackTransactionAsync(cancellationToken);
            Logger.LogInformation("Transaction rolled back");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DbContext.Database.BeginTransactionAsync(cancellationToken);
            Logger.LogInformation("Transaction started");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error beginning transaction");
            throw;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
