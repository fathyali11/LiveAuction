using CleanArchitectureSolutionName.Domain.Repositories;
using CleanArchitectureSolutionName.Infrastructure.Presistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureSolutionName.Infrastructure.Repositories;
internal class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T: class
{
    private readonly ApplicationDbContext _context = context;

    public async Task<T> AddAsync(T entity,CancellationToken cancellationToken)
    {
        await _context.Set<T>().AddAsync(entity,cancellationToken);
        return entity;
    }
    public void DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }
    public async Task<T?> GetByIdAsync(int id,CancellationToken cancellationToken)
    {
        return await _context.Set<T>().FindAsync([id],cancellationToken);
    }
}
