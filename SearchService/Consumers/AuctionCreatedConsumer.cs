using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;


public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
	private readonly IMapper _mapper;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mapper"> AutoMapper service </param>
	public AuctionCreatedConsumer(IMapper mapper)
	{
		_mapper = mapper;
	}

	/// <summary>
	/// If the SearchService is down, any AuctionCreated message published to the AuctionCreated queue 
	/// will be consumed when the SearchService is back up again.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public async Task Consume(ConsumeContext<AuctionCreated> context)
	{
		Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

		var item = _mapper.Map<Item>(context.Message);

		await item.SaveAsync();	// If MongoDB is down, we will not have data concistency between AuctionService DB and SearchService DB
	}
}
