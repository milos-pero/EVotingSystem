using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EVotingSystem.Models;

namespace EVotingSystem.Persistence
{
    public static class UserRepository
    {
        private static readonly string DataDir = "Data";
        private static readonly string UsersFile = Path.Combine(DataDir, "users.json");

        static UserRepository()
        {
            Directory.CreateDirectory(DataDir);

            if (!File.Exists(UsersFile))
                File.WriteAllText(UsersFile, "[]");
        }

        // ---------------------------
        // PUBLIC API
        // ---------------------------

        public static void AddUser(User user)
        {
            var users = LoadAllInternal();
            users.Add(ToDto(user));
            SaveAllInternal(users);
        }
        public static List<User> GetAllUsers()
        {
            return LoadAllDtos().Select(FromDto).ToList();
        }

        public static User? FindByPublicCertPath(string publicCertPath)
        {
            var users = LoadAllInternal();

            var dto = users.FirstOrDefault(u =>
                string.Equals(u.PublicCertPath, publicCertPath, StringComparison.OrdinalIgnoreCase));

            return dto == null ? null : FromDto(dto);
        }

        public static User? FindByLogin(string login)
        {
            var users = LoadAllInternal();

            var dto = users.FirstOrDefault(u =>
                u.UserType == UserType.Organizer && u.OrganizationName == login ||
                u.UserType == UserType.Voter && u.Username == login);

            return dto == null ? null : FromDto(dto);
        }
        public static User? FindById(Guid id)
        {
            var users = LoadAllInternal();

            var dto = users.FirstOrDefault(u => u.Id == id);
            return dto == null ? null : FromDto(dto);
        }


        // ---------------------------
        // INTERNAL STORAGE
        // ---------------------------

        private static List<UserDto> LoadAllInternal()
        {
            string json = File.ReadAllText(UsersFile);
            return JsonSerializer.Deserialize<List<UserDto>>(json)!;
        }

        private static void SaveAllInternal(List<UserDto> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(UsersFile, json);
        }

        // ---------------------------
        // DTO MAPPING
        // ---------------------------

        private static UserDto ToDto(User user)
        {
            if (user is Organizer o)
            {
                return new UserDto
                {
                    UserType = UserType.Organizer,
                    Id = o.Id,
                    Password = o.Password,
                    KeySalt = o.KeySalt,
                    CertificatePath = o.CertificatePath,
                    PublicCertPath = o.PublicCertPath,
                    OrganizationName = o.OrganizationName,
                    OrganizationId = o.OrganizationId
                };
            }

            if (user is Voter v)
            {
                return new UserDto
                {
                    UserType = UserType.Voter,
                    Id = v.Id,
                    Password = v.Password,
                    KeySalt = v.KeySalt,
                    CertificatePath = v.CertificatePath,
                    PublicCertPath = v.PublicCertPath,
                    FirstName = v.FirstName,
                    LastName = v.LastName,
                    Username = v.Username
                };
            }

            throw new InvalidOperationException("Unknown user type");
        }

        private static List<UserDto> LoadAllDtos()
        {
            string json = File.ReadAllText(UsersFile);
            return JsonSerializer.Deserialize<List<UserDto>>(json)!;
        }

        private static User FromDto(UserDto dto)
        {
            if (dto.UserType == UserType.Organizer)
            {
                return new Organizer
                {
                    Id = dto.Id,
                    Password = dto.Password,
                    KeySalt = dto.KeySalt,
                    CertificatePath = dto.CertificatePath,
                    PublicCertPath = dto.PublicCertPath,
                    OrganizationName = dto.OrganizationName!,
                    OrganizationId = dto.OrganizationId!
                };
            }

            if (dto.UserType == UserType.Voter)
            {
                return new Voter
                {
                    Id = dto.Id,
                    Password = dto.Password,
                    KeySalt = dto.KeySalt,
                    CertificatePath = dto.CertificatePath,
                    PublicCertPath = dto.PublicCertPath,
                    FirstName = dto.FirstName!,
                    LastName = dto.LastName!,
                    Username = dto.Username!
                };
            }

            throw new InvalidOperationException("Unknown user type");
        }

        public static User? FindUserForLogin(string loginInput)
        {
            var users = LoadAllDtos();

            foreach (var dto in users)
            {
                if (dto.UserType == UserType.Organizer &&
                    dto.OrganizationName != null &&
                    dto.OrganizationName.Equals(loginInput, StringComparison.OrdinalIgnoreCase))
                {
                    return FromDto(dto);
                }

                if (dto.UserType == UserType.Voter &&
                    dto.Username != null &&
                    dto.Username.Equals(loginInput, StringComparison.OrdinalIgnoreCase))
                {
                    return FromDto(dto);
                }
            }

            return null;
        }

        // ---------------------------
        // DTO CLASS
        // ---------------------------

        private class UserDto
        {
            public UserType UserType { get; set; }
            public Guid Id { get; set; }
            public string Password { get; set; }
            public byte[] KeySalt { get; set; }
            public string CertificatePath { get; set; }
            public string PublicCertPath { get; set; }

            // Organizer
            public string? OrganizationName { get; set; }
            public string? OrganizationId { get; set; }

            // Voter
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Username { get; set; }
        }
    }
}
