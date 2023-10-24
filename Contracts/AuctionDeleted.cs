// MassTransit uses the full type name, including the namespace, for message contracts.
// When creating the same message type in two separate projects, the namespaces must match or the message will not be consumed.
namespace Contracts;

public class AuctionDeleted
{
	public string Id { get; set; }
}
