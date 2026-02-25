namespace StudentManagement.Exceptions;

public sealed class InvalidAgeException : Exception
{
    public InvalidAgeException(string message) : base(message) { }
}