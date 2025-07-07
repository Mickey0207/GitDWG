using System;
using System.Collections.Generic;
using System.Linq;

namespace GitDWG.Models
{
    public class AppUser
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UserAuthenticationService
    {
        private readonly List<AppUser> _predefinedUsers;
        private const string ADMIN_PASSWORD = "ACL53959233";

        public UserAuthenticationService()
        {
            _predefinedUsers = new List<AppUser>
            {
                new AppUser { Name = "Sean", Password = "Sean123" },
                new AppUser { Name = "William", Password = "William123" },
                new AppUser { Name = "Steven", Password = "Steven123" },
                new AppUser { Name = "Janice", Password = "Janice123" },
                new AppUser { Name = "Eason", Password = "Eason123" },
                new AppUser { Name = "Mickey", Password = "Mickey123" },
                new AppUser { Name = "Emma", Password = "Emma123" },
                new AppUser { Name = "Vincent", Password = "Vincent123" },
                new AppUser { Name = "Billy", Password = "Billy123" },
                new AppUser { Name = "May", Password = "May123" },
                new AppUser { Name = "Todd", Password = "Todd123" },
                new AppUser { Name = "Jake", Password = "Jake123" },
                new AppUser { Name = "Henry", Password = "Henry123" },
                new AppUser { Name = "Ryder", Password = "Ryder123" },
                new AppUser { Name = "Dennis", Password = "Dennis123" },
                new AppUser { Name = "Jeff", Password = "Jeff123" },
                new AppUser { Name = "Liz", Password = "Liz123" },
                new AppUser { Name = "Zoey", Password = "Zoey123" },
                new AppUser { Name = "Jin", Password = "Jin123" }
            };
        }

        public List<AppUser> GetAllUsers()
        {
            return _predefinedUsers.Where(u => u.IsActive).ToList();
        }

        public bool AuthenticateUser(string userName, string password)
        {
            var user = _predefinedUsers.FirstOrDefault(u => 
                u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase) && 
                u.IsActive);
            
            if (user != null && user.Password == password)
            {
                user.LastLogin = DateTime.Now;
                return true;
            }
            return false;
        }

        public bool AuthenticateAdmin(string password)
        {
            return password == ADMIN_PASSWORD;
        }

        public bool AddUser(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return false;

            if (_predefinedUsers.Any(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase)))
                return false;

            _predefinedUsers.Add(new AppUser 
            { 
                Name = userName, 
                Password = password,
                LastLogin = DateTime.Now
            });
            return true;
        }

        public AppUser? GetUser(string userName)
        {
            return _predefinedUsers.FirstOrDefault(u => 
                u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase) && 
                u.IsActive);
        }
    }
}