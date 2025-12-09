using Models;
using System.Collections.Generic;

namespace Services
{
    public interface IUserService
    {
        List<User> GetAll();
        User Get(int id);
        void Add(User s);
        void Update(User s);
        void Delete(int id);
        int Count { get; }
    }
}
