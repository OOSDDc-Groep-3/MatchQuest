using System;

namespace MatchQuest.Core.Models
{
    public partial class Client : Model
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; } = Role.None;

        public DateTime? BirthDate { get; set; }
        public string? Region { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = true;

        public Client(int id, string name, string emailAddress, string password) : base(id, name)
        {
            EmailAddress = emailAddress;
            Password = password;
        }

        // Optional full constructor to populate all columns from DB
        public Client(int id, string name, string emailAddress, string password,
            DateTime? birthDate, string? region, string? bio, string? profilePicture, bool isActive) : base(id, name)
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