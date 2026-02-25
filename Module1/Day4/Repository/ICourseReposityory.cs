using StudentManagement.Domain;

namespace StudentManagement.Repository;

public interface ICourseRepository : IRepository<Course, Guid>
{
    Course? GetByCode(string code);
    int GetActiveEnrollmentCount(Guid courseId);
    IEnumerable<Course> Search(string? term = null);
}