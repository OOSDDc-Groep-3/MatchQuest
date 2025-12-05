using System;
using System.Collections.Generic;

namespace MatchQuest.Core.Models
{
    public partial class User : Model
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; } = Role.None;
        public DateOnly? BirthDate { get; set; }
        public List<Game> Games { get; set; } = [];
        public List<Like> Likes { get; set; } = [];
        public List<Match> Matches { get; set; } = [];
        public string? Region { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = true;

        public User(int id, string name, string emailAddress, string password) : base(id, name)
        {
            EmailAddress = emailAddress;
            Password = password;
        }

        // Optional full constructor to populate all columns from DB
        public User(int id, string name, string emailAddress, string password,
            DateOnly? birthDate, string? region, string? bio, string? profilePicture, bool isActive) : base(id, name)
        {
            EmailAddress = emailAddress;
            Password = password;
            BirthDate = birthDate;
            Region = region;
            Bio = bio;
            ProfilePicture = profilePicture;
            IsActive = isActive;
        }
    }
}