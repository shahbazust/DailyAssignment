using StudentManagement.Exceptions;

namespace StudentManagement.Domain;

public sealed class Course
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Code { get; private set; }
    public string Title { get; private set; }
    public int Credits { get; private set; }

    
    public Course(string code, string title, int credits)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Code is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");

        if (credits <= 0)
            throw new DomainException("Credits must be > 0.");

        Code = code.Trim().ToUpper();
        Title = title.Trim();
        Credits = credits;
    }

    
    public void Update(string? title = null, int? credits = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();

        if (credits.HasValue && credits.Value > 0)
            Credits = credits.Value;
    }

    public override string ToString() => $"{Code} - {Title} ({Credits} cr)";
}