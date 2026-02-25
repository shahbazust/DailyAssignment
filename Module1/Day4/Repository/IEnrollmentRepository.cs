using StudentManagement.Domain;

namespace StudentManagement.Repository
{
    public interface IEnrollmentRepository : IRepository<Enrollment, Guid>
    {
        // Query by foreign keys
        IEnumerable<Enrollment> GetByStudent(Guid studentId);
        IEnumerable<Enrollment> GetByCourse(Guid courseId);

        // Active enrollment for a (student, course) pair
        Enrollment? GetActive(Guid studentId, Guid courseId);

        // Flexible search/filter
        IEnumerable<Enrollment> Search(
            Guid? studentId = null,
            Guid? courseId = null,
            bool? isActive = null,
            Grade? grade = null
        );
    }
}