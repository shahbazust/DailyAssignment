using StudentManagement.Domain;
using StudentManagement.Exceptions;
using StudentManagement.Logging;
using StudentManagement.Repository;

namespace StudentManagement.Service;

public sealed class EnrollmentService
{
    private readonly IStudentRepository _students;
    private readonly ICourseRepository _courses;
    private readonly IList<Enrollment> _enrollments;
    private readonly IAppLogger _logger;

    public EnrollmentService(
        IStudentRepository students,
        ICourseRepository courses,
        IList<Enrollment> enrollments,
        IAppLogger logger)
    {
        _students = students;
        _courses = courses;
        _enrollments = enrollments;
        _logger = logger;
    }

    public Enrollment Enroll(Guid studentId, Guid courseId)
    {
        _logger.Info($"Enroll request: student={studentId}, course={courseId}");

        var student = _students.GetById(studentId) ?? throw new NotFoundException("Student not found");
        if (!student.IsActive) throw new DomainException("Student is inactive");

        var course = _courses.GetById(courseId) ?? throw new NotFoundException("Course not found");

        var already = _enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId && e.IsActive);
        if (already) throw new DuplicateEntityException("Student already enrolled in this course");

        var countActive = _courses.GetActiveEnrollmentCount(courseId);

        var enrollment = new Enrollment(studentId, courseId);
        _enrollments.Add(enrollment);
        _logger.Info($"Enrollment created: {enrollment.Id}");
        return enrollment;
    }

    public void AssignGrade(Guid enrollmentId, Grade grade)
    {
        var e = _enrollments.FirstOrDefault(x => x.Id == enrollmentId)
                ?? throw new NotFoundException("Enrollment not found");
        e.AssignGrade(grade);
        _logger.Info($"Assigned grade {grade} to enrollment {enrollmentId}");
    }

    public void Drop(Guid enrollmentId)
    {
        var e = _enrollments.FirstOrDefault(x => x.Id == enrollmentId)
                ?? throw new NotFoundException("Enrollment not found");
        e.Drop();
        _logger.Info($"Dropped enrollment {enrollmentId}");
    }
}