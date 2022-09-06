using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using LiteDB;
using EmbedIO.Samples;

public class EventModel
{
    [BsonId]
    public Guid Id { get; set; }

    public string EventText { get; set; }

    [BsonRef("Variants")]
    public List<VariantModel> Variants { get; set; }

    [BsonIgnore]
    public string SpriteName => $"EventImg_{Id}.png";


    public static async Task<IEnumerable<EventModel>> GetDataAsync()
    {
        return Database.Instance.GetEvents();
    }
}
