using MongoDB.Entities;

namespace SearchService.Models;

/// <summary>
///		Deriving from MongoDB.Entity because Item is stored in MongoDB. 
///		Using MongoDB.Entity to make process more Entity Framwork alike.
/// </summary>
public class Item : Entity
{
	/// <summary>
	///		MongoDB.Entity already has an ID property
	/// </summary>
	//public string ID { get; set; }

	public int ReservePrice { get; set; }
	public string Seller { get; set; }
	public string Winner { get; set; }
	public int SoldAmount { get; set; }
	public int CurrentHighBid { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public DateTime AuctionEnd { get; set; }
	public string Status { get; set; }
	public string Make { get; set; }
	public string Model { get; set; }
	public int Year { get; set; }
	public string Color { get; set; }
	public int Mileage { get; set; }
	public string ImageUrl { get; set; }
}
