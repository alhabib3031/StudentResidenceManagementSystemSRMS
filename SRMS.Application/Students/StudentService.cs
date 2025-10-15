using System.Linq.Expressions;
using SRMS.Domain.Students;

namespace SRMS.Application.Students;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }
    
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _studentRepository.GetAllAsync();
    }
    
    public Task<Student?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
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
    
    public Task<Student> CreateAsync(Student entity)
    {
        throw new NotImplementedException();
    }
    
    public Task<Student> UpdateAsync(Student entity)
    {
        throw new NotImplementedException();
    }
    
    public Task<bool> DeleteAsync(Student entity)
    {
        throw new NotImplementedException();
    }
    
    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}