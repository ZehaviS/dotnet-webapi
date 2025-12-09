using Models;
using System.Collections.Generic;

namespace Services
{
    public interface ISaledsService
    {
        List<Saleds> GetAll();
        Saleds Get(int id);
        void Add(Saleds s);
        void Update(Saleds s);
        void Delete(int id);
        int Count { get; }
    }
}
