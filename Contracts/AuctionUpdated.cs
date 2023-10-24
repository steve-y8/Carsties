// MassTransit uses the full type name, including the namespace, for message contracts.
// When creating the same message type in two separate projects, the namespaces must match or the message will not be consumed.
namespace Contracts;

public class AuctionUpdated
{
	public string Id { get; set; }

	public string Make { get; set; }

	public string Model { get; set; }

	public int Year { get; set; }

	public string Color { get; set; }

	public int Mileage { get; set; }
}
