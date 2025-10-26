using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Infrastructure.Repositories;

public class StudentRepository : IRepositories<Student>
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Query Operations
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
            .Where(s => !s.IsDeleted)
            .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(Guid id)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public Task<Student?> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Student>> FindAsync(Expression<Func<Student, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<Student?> FirstOrDefaultAsync(Expression<Func<Student, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    // Command Operations
    public async Task<Student> CreateAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await SaveChangesAsync();
        return student;
    }

    public async Task<Student> UpdateAsync(Student student)
    {
        student.UpdatedAt = DateTime.UtcNow;
        _context.Students.Update(student);
        await SaveChangesAsync();
        return student;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var student = await GetByIdAsync(id);
        if (student == null) return false;
        
        // Softly Delete
        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        student.IsActive = false;
        
        await UpdateAsync(student);
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}