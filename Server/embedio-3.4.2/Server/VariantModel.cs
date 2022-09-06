using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using LiteDB;
using EmbedIO.Samples;

[Serializable]
public class VariantModel
{
    [BsonId]
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ResultText { get; set; }

    public List<Goods> NeedGoods { get; set; }
    public List<Goods> RecievingGoods { get; set; }

    public static async Task<IEnumerable<VariantModel>> GetDataAsync()
    {
        return Database.Instance.GetVariants();
    }
}