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
    public class SaledServiceJson : ISaledsService
    {
        List<Saleds> Saleds { get; }
        //private IWebHostEnvironment  webHost;
        private string filePath;
        public SaledServiceJson(IWebHostEnvironment webHost)
        {
            //this.webHost = webHost;
            this.filePath = Path.Combine(webHost.ContentRootPath, "Data", "Saled.json");
            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                Saleds = JsonSerializer.Deserialize<List<Saleds>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })?? new List<Saleds>();;
            }
        }

        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(Saleds);
            File.WriteAllText(filePath, text);
        }
        public List<Saleds> GetAll() => Saleds;

        public Saleds Get(int id) => Saleds.FirstOrDefault(p => p.Id == id);

        public void Add(Saleds saled)
        {
            saled.Id = Saleds.Count() + 1;
            Saleds.Add(saled);
            saveToFile();
        }

        public void Delete(int id)
        {
            var saled = Get(id);
            if (saled is null)
                return;

            Saleds.Remove(saled);
            saveToFile();
        }

        public void Update(Saleds saled)
        {
            var index = Saleds.FindIndex(p => p.Id == saled.Id);
            if (index == -1)
                return;

            Saleds[index] = saled;
            saveToFile();
        }

        public int Count => Saleds.Count();
    }
}