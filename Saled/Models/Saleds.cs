namespace Models
{
    public class Saleds
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double weight { get; set; }

        // Tracks which user created/owns this record
        public int UserId { get; set; }

        // Optional image shown next to the salad name (relative to wwwroot)
        public string ImageUrl { get; set; }
    }
}
