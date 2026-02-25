namespace StudentManagement.Exceptions;

public sealed class DuplicateEntityException : Exception
{
    public DuplicateEntityException(string message) : base(message) { }
}