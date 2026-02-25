namespace StudentManagement.Domain;

public abstract class BasePerson
{
    public Guid Id { get; protected set; }
    public string FirstName { get; protected set; }
    public string LastName { get; protected set; }
    public string FullName => $"{FirstName} {LastName}".Trim();

    protected BasePerson(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name required", nameof(firstName));

        Id = Guid.NewGuid();
        FirstName = firstName.Trim();
        LastName = lastName?.Trim() ?? string.Empty;
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name required", nameof(firstName));
        FirstName = firstName.Trim();
        LastName = lastName?.Trim() ?? string.Empty;
    }

    public override string ToString() => $"{FullName} ({Id})";
}