using Microsoft.EntityFrameworkCore;
using SRMS.Application.Colleges;
using SRMS.Domain.Colleges;

namespace SRMS.Infrastructure.Configurations.Services;

public class CollegeService : ICollegeService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public CollegeService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<College>> GetAllCollegesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Colleges.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<College?> GetCollegeByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Colleges.FindAsync(id);
    }

    public async Task<bool> CreateCollegeAsync(College college)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Colleges.Add(college);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateCollegeAsync(College college)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Colleges.Update(college);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteCollegeAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var college = await context.Colleges.FindAsync(id);
        if (college == null) return false;
        context.Colleges.Remove(college);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<List<Major>> GetMajorsByCollegeAsync(Guid collegeId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Majors
            .Where(m => m.CollegeId == collegeId)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<bool> CreateMajorAsync(Major major)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Majors.Add(major);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteMajorAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var major = await context.Majors.FindAsync(id);
        if (major == null) return false;
        context.Majors.Remove(major);
        return await context.SaveChangesAsync() > 0;
    }
}
