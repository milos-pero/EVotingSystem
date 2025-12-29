using System.Collections.Generic;
using EVotingSystem.Models;

namespace EVotingSystem.Security
{
    public static class UserRepository
    {
        private static readonly List<User> users = new();

        public static void AddUser(User user)
        {
            users.Add(user);
        }

        public static User? GetUserByName(string name)
        {
            foreach (var user in users)
            {
                switch (user)
                {
                    case Organizer o when o.OrganizationName == name:
                        return o;
                    case Voter v when v.Username == name:
                        return v;
                }
            }
            return null;
        }

        public static IEnumerable<User> GetAllUsers() => users;
    }
}
