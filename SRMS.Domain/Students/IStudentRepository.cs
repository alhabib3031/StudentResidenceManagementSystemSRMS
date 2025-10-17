namespace SRMS.Domain.Students;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(Guid id);
    
    // Command Operations
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateAsync(Student student);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}