using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.Data;

public class DbInitializer
{
	public static async Task InitDb(WebApplication app)
	{
		// Only MongoDB needs to be initialized.
		// MYSQL and Postgres only need to add DbContext into app's IServiceCollection.
		await DB.InitAsync(
			"SearchDb",
			MongoClientSettings.FromConnectionString(
				app.Configuration.GetConnectionString("MongoDbConnection")
			)
		);

		// Set wich property can be search on
		// Don't know is this a MongoDB process
		// Need to learn how to implement a search service using other database
		await DB.Index<Item>()
			.Key(x => x.Make, KeyType.Text)
			.Key(x => x.Model, KeyType.Text)
			.Key(x => x.Color, KeyType.Text)
			.Key(x => x.Status, KeyType.Text)
			.Key(x => x.Year, KeyType.Text)
			.Key(x => x.Mileage, KeyType.Text)
			.CreateAsync();

		// Seeding data is just for testing purpose
		var count = await DB.CountAsync<Item>();

		if(count == 0)
		{
			Console.WriteLine("No data - will attempt to seed");

			var itemData = await File.ReadAllTextAsync("Data/auctions.json");

			var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};

			var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

			await DB.SaveAsync(items);
		}
	}
}
