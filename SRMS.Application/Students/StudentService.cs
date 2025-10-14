using System.Linq.Expressions;
using SRMS.Domain.Students;

namespace SRMS.Application.Students;

public class StudentService : IStudentService
{
    public Task<IEnumerable<Student>> GetAllAsync()
    {
        var students = new List<Student>
        {
            new Student
            {
                Id = Guid.NewGuid(),
                FirstName = "Alhab1",
                LastName = "Haque1",
                Email = "<EMAIL>",
                PhoneNumber = "01712345678",
                Address = "Dhaka",
                Image = null
            },
            new Student
            {
                Id = Guid.NewGuid(),
                FirstName = "Alhab2",
                LastName = "Haque2",
                Email = "<EMAIL>",
                PhoneNumber = "01712345678",
                Address = "Dhaka",
            }
        };

        return Task.FromResult<IEnumerable<Student>>(students);
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