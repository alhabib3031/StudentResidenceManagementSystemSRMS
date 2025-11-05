using System.Linq.Expressions;
using SRMS.Domain.Managers;
using SRMS.Domain.Repositories;

namespace SRMS.Infrastructure.Repositories;

public class ManagerRepository : IRepositories<Manager>
{
    public Task<IEnumerable<Manager>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Manager?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Manager?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Manager>> FindAsync(Expression<Func<Manager, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Manager?> FirstOrDefaultAsync(Expression<Func<Manager, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Manager> CreateAsync(Manager entity)
    {
        throw new NotImplementedException();
    }

    public Task<Manager> UpdateAsync(Manager entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}