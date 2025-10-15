namespace SRMS.Domain.Students;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();
}