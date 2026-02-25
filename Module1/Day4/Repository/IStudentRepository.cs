using StudentManagement.Domain;

namespace StudentManagement.Repository;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Student? GetByEmail(string email);
    IEnumerable<Student> Search(string? name = null, string? emailDomain = null, bool? isActive = null);
}