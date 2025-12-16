using System;
using System.Collections.Generic;

namespace MatchQuest.Core.Models
{
    public partial class User : Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; } = Role.None;
        public DateOnly? BirthDate { get; set; }
        public List<Game> Games { get; set; } = new List<Game>();
        public List<Reaction> Reactions { get; set; } = new List<Reaction>();
        public List<Match> Matches { get; set; } = new List<Match>();
        public List<Chat> Chats { get; set; } = new List<Chat>();
        public string? Region { get; set; }
        public string? Biography { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? LastMessagePreview { get; set; }

        public User(int id, string name, string email, string password) : base(id, name)
        {
            Name = name;
            Email = email;
            Password = password;
        }

        // Optional full constructor to populate all columns from DB
        public User(int id, string name, string email, string password,
            DateOnly? birthDate, string? region, string? biography, string? profilePicture, bool isActive, DateTime createdAt, DateTime? updatedAt) : base(id, name)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            BirthDate = birthDate;
            Region = region;
            Biography = biography;
            ProfilePicture = profilePicture;
            IsActive = isActive;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public int GetAge()
        {
            if (BirthDate == null)
                return 0;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            int age = today.Year - BirthDate.Value.Year;

            if (today < BirthDate.Value.AddYears(age))
                age--;

            return age;
        }
    }
}