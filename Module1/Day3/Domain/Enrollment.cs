namespace StudentManagement.Domain;

public sealed class Enrollment
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public DateTime EnrolledOn { get; } = DateTime.Now;
    public Grade Grade { get; private set; } = Grade.NotGraded;
    public bool IsActive { get; private set; } = true; 

    public Enrollment(Guid studentId, Guid courseId)
    {
        if (studentId == Guid.Empty || courseId == Guid.Empty)
            throw new ArgumentException("Invalid foreign keys");
        StudentId = studentId;
        CourseId = courseId;
    }

    public void AssignGrade(Grade grade)
    {
        if (!IsActive) throw new Exception("Cannot grade inactive enrollment");
        Grade = grade;
        if (grade != Grade.NotGraded)
            IsActive = false; // completed
    }

    public void Drop()
    {
        if (!IsActive) throw new Exception("Already inactive");
        Grade = 0;
        IsActive = false;
    }

    public override string ToString() =>
        $"Enrollment {Id} S:{StudentId} C:{CourseId} Grade:{Grade} Active:{IsActive}";
}
