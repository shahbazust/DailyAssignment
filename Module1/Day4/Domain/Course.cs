using System;
using System.Text.Json.Serialization;
using StudentManagement.Exceptions;
using System.Text.RegularExpressions;

namespace StudentManagement.Domain
{
    public  class Course
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Code { get; private set; }
        public string Title { get; private set; }
        public int Credits { get; private set; }

        public Course(string code, string title, int credits)
        {
            Code = code.Trim().ToUpperInvariant();
            Title = title.Trim();
            Credits = credits;
        }

        [JsonConstructor]
        public Course(Guid id, string code, string title, int credits)
        {
            Id = id;
            Code = (code ?? "").Trim().ToUpperInvariant();
            Title = (title ?? "").Trim();
            Credits = credits;
        }

        public void Update(string? title = null, int? credits = null)
        {
            if (!string.IsNullOrWhiteSpace(title)) Title = title.Trim();
            if (credits.HasValue && credits.Value > 0) Credits = credits.Value;
        }

        public override string ToString() => $"{Code} - {Title} ({Credits} cr)";
    }
}