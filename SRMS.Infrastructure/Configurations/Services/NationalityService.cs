using Microsoft.EntityFrameworkCore;
using SRMS.Application.Common.Interfaces;
using SRMS.Domain.Common;
using SRMS.Infrastructure;

namespace SRMS.Infrastructure.Configurations.Services;

public class NationalityService : INationalityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public NationalityService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Nationality>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Nationalities.OrderBy(n => n.Name).ToListAsync();
    }

    public async Task<Nationality?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Nationalities.FindAsync(id);
    }

    public async Task<bool> CreateAsync(Nationality nationality)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Nationalities.Add(nationality);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Nationality nationality)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Nationalities.Update(nationality);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var nationality = await context.Nationalities.FindAsync(id);
        if (nationality == null) return false;

        context.Nationalities.Remove(nationality);
        return await context.SaveChangesAsync() > 0;
    }
}
