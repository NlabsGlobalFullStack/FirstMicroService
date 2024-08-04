using FirstMicroService.Orders.WebAPI.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FirstMicroService.Orders.WebAPI.Context;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }
}
