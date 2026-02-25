using System;
using System.Text.Json.Serialization;
using StudentManagement.Exceptions;

namespace StudentManagement.Domain
{
    public sealed class Student : BasePerson
    {
        public string Email { get; private set; } = string.Empty;
        public DateTime DateOfBirth { get; private set; }
        public int Age
        {
            get
            {
                var today = DateTime.Now.Date;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public DateTime EnrolledOn { get; } = DateTime.Now;
        public bool IsActive { get; private set; } = true;

        public Student(string firstName, string lastName, string email, DateTime dateOfBirth)
            : base(firstName, lastName)
        {
            UpdateEmail(email);
            SetDateOfBirth(dateOfBirth);
            EnsureMinimumAge(16);
        }
        [JsonConstructor]
        public Student(Guid id, string firstName, string lastName, string email, DateTime dateOfBirth, DateTime enrolledOn, bool isActive)
            : base(firstName, lastName)
        {
            Id = id; // protected set in BasePerson
            UpdateEmail(email);
            SetDateOfBirth(dateOfBirth);
            EnrolledOn = enrolledOn;
            IsActive = isActive;
        }

        private void EnsureMinimumAge(int minAge = 18)
        {
            if (Age < minAge)
                throw new InvalidAgeException($"Student must be at least {minAge} years old. Current age: {Age}");
        }

        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new DomainException("Invalid email format.");
            Email = email.Trim();
        }

        public void SetDateOfBirth(DateTime dob) => DateOfBirth = dob.Date;

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}