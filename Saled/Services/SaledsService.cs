using Models;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class SaledsService : ISaledsService
    {
        private static List<Saleds> _saleds;
        private static int _nextId = 6;

        static SaledsService()
        {
            _saleds = new List<Saleds>
            {
                new Saleds { Id = 1, Name = "גזר מרוקאי", weight = 2.5 },
                new Saleds { Id = 2, Name = "גזר בשום שמיר", weight = 3.0 },
                new Saleds { Id = 3, Name = "זיתים מרוקאים", weight = 4.2 },
                new Saleds { Id = 4, Name = "סלט חצילים", weight = 1.5 },
                new Saleds { Id = 5, Name = "סלט מלפפונים", weight = 2.0 }
            };
        }

        public List<Saleds> GetAll() => _saleds;

        public Saleds Get(int id) =>
            _saleds.FirstOrDefault(s => s.Id == id);

        public void Add(Saleds saled)
        {
            saled.Id = _nextId++;
            _saleds.Add(saled);
        }

        public void Update(Saleds saled)
        {
            var index = _saleds.FindIndex(s => s.Id == saled.Id);
            if (index == -1)
                return;

            _saleds[index] = saled;
        }

        public void Delete(int id)
        {
            var saled = Get(id);
            if (saled != null)
                _saleds.Remove(saled);
        }

        public int Count => _saleds.Count;
    }
}
