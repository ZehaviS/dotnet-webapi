using Models;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class UserService : IUserService
    {
        private static List<User> _users;
        private static int _nextId = 6;

        static UserService()
        {
            _users = new List<User>
            {
                new User { Id = 1, Name = " אילה" },
                new User { Id = 2, Name = " זהבי "},
                new User { Id = 3, Name = " מירי" },
                new User { Id = 4, Name = "שירה " },
                new User { Id = 5, Name = "מיכל " }
            };
        }

        public List<User> GetAll() => _users;

        public User Get(int id) =>_users.FirstOrDefault(s => s.Id == id);

        public void Add(User user)
        {
            user.Id = _nextId++;
            _users.Add(user);
        }

        public void Update(User user)
        {
            var index = _users.FindIndex(s => s.Id == user.Id);
            if (index == -1)
                return;

            _users[index] = user;
        }

        public void Delete(int id)
        {
            var user = Get(id);
            if (user != null)
                _users.Remove(user);
        }

        public int Count => _users.Count;
    }
}
