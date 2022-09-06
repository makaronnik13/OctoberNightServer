using LiteDB;

namespace EmbedIO.Samples
{
     public class LocationModel
    {
        [BsonId]
        public Guid Id { get; set; }

        [BsonIgnore]
        public string SpriteName => $"LocationImg_{Id}.png";

        public string LocationName { get; set; }

        public Position2d Position { get; set; }

        [BsonRef("Users")]
        public User? Owner { get; set; }

        [BsonRef("Events")]
        public List<EventModel> Events { get;  set;}


        public static async Task<IEnumerable<LocationModel>> GetDataAsync()
        {
            return Database.Instance.GetPoints();
        }
    }

}