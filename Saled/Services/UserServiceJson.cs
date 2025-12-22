using Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Services
{
    public class UserServiceJson : IUserService
    {
        List<User> Users { get; }
        //private IWebHostEnvironment  webHost;
        private string filePath;
        public UserServiceJson(IWebHostEnvironment webHost)
        {
            //this.webHost = webHost;
            this.filePath = Path.Combine(webHost.ContentRootPath, "Data", "User.json");
            string content;
            using (var jsonFile = File.OpenText(filePath))
            {
                content = jsonFile.ReadToEnd();
            }
            Users = JsonSerializer.Deserialize<List<User>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<User>();
        }

        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(Users);
            File.WriteAllText(filePath, text);
        }
        public List<User> GetAll() => Users;

        public User Get(int id) => Users.FirstOrDefault(p => p.Id == id);

        public void Add(User user)
        {
            user.Id = Users.Count() + 1;
            Users.Add(user);
            saveToFile();
        }

        public void Delete(int id)
        {
            var user = Get(id);
            if (user is null)
                return;

            Users.Remove(user);
            saveToFile();
        }

        public void Update(User user)
        {
            var index = Users.FindIndex(p => p.Id == user.Id);
            if (index == -1)
                return;

            Users[index] = user;
            saveToFile();
        }

        public int Count => Users.Count();
    }
}