using Infrastructure.Data.Postgres.Entities.Base.Interface;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories;
using Infrastructure.Data.Postgres.Repositories.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Event;

namespace Infrastructure.Data.Postgres;

public class UnitOfWork : IUnitOfWork
{
    private readonly PostgresContext _postgresContext;
    private readonly IMediator       _mediator;

    private UserRepository? _userRepository;
    private UserTokenRepository? _userTokenRepository;
    private ProductRepository? _productRepository;
    private ProductSupplyRepository? _productSupplyRepository;
    private ProductSaleRepository? _productSaleRepository;
    private OrganizationRepository? _organizationRepository;
    private OrderRepository? _orderRepository;
    private CategoryRepository? _categoryRepository;

    public IUserRepository Users => _userRepository ??= new UserRepository(_postgresContext);
    public IUserTokenRepository UserTokens => _userTokenRepository ??= new UserTokenRepository(_postgresContext);
    public IProductRepository Products => _productRepository ??= new ProductRepository(_postgresContext);
    public IProductSupplyRepository ProductSupplies => _productSupplyRepository ??= new ProductSupplyRepository(_postgresContext);
    public IProductSaleRepository ProductSales => _productSaleRepository ??= new ProductSaleRepository(_postgresContext);
    public IOrganizationRepository Organizations => _organizationRepository ??= new OrganizationRepository(_postgresContext);
    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_postgresContext);
    public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_postgresContext);


    public UnitOfWork(PostgresContext postgresContext, IMediator mediator)
    {
        _postgresContext = postgresContext;
        _mediator        = mediator;
    }

    public async Task<int> CommitAsync()
    {
        var updatedEntities = _postgresContext.ChangeTracker.Entries<ITrackedEntity>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var updatedEntity in updatedEntities)
        {
            updatedEntity.UpdatedAt = DateTime.UtcNow;
        }

        var result = await _postgresContext.SaveChangesAsync();

        if (result > 0)
        {
            var entitiesWithEvents = _postgresContext.ChangeTracker.Entries<EntityWithEvents>()
                .Select(e => e.Entity).ToArray();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.GetEventsToPublish();

                foreach (var @event in events)
                {
                    await _mediator.Publish(@event);
                }
            }
        }

        return result;
    }

    public void Dispose()
    {
        _postgresContext.Dispose();
    }
}
