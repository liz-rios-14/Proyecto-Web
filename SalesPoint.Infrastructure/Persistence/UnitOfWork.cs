using Microsoft.EntityFrameworkCore.Storage;
using SalesPoint.Application.Interfaces.Repositories;

namespace SalesPoint.Infrastructure.Persistence;
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    public UnitOfWork(AppDbContext context) => _context = context;
    public async Task BeginTransactionAsync() => _transaction ??= await _context.Database.BeginTransactionAsync();
    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
        if (_transaction is not null) { await _transaction.CommitAsync(); await _transaction.DisposeAsync(); _transaction = null; }
    }
    public async Task RollbackAsync()
    {
        if (_transaction is not null) { await _transaction.RollbackAsync(); await _transaction.DisposeAsync(); _transaction = null; }
    }
    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
