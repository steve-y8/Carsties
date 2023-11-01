using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
	private readonly AuctionDbContext _context;

	private readonly IMapper _mapper;

	private readonly IPublishEndpoint _publishEndpoint;

	public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}

	[HttpGet]
	public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
	{
		var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
		return _mapper.Map<List<AuctionDto>>(auctions);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
	{
		var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
		if (auction == null)
		{
			return NotFound();
		}
		return _mapper.Map<AuctionDto>(auction);
	}

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
	{
		var auction = _mapper.Map<Auction>(auctionDto);
		
		auction.Seller = User.Identity.Name;	// User is a property in ControllerBase class
												// User.Identity.Name is the NameClaimType configured in the Authentication service (Program.cs)

		_context.Auctions.Add(auction);

		var result = await _context.SaveChangesAsync() > 0;	// Will throw exception if DB is down

		var newAuction = _mapper.Map<AuctionDto>(auction);

		// Publish AuctionCreated message to the message queue
		// If RabbitMQ is down, we will not have data concistency between AuctionService DB and SearchService DB
		await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

		if (!result)
		{
			return BadRequest("Could not save changes to the DB");
		}

		return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
	}

	[Authorize]
	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
	{
		var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

		if (auction == null)
		{
			return NotFound();
		}

		if (auction.Seller != User.Identity.Name) return Forbid();	// User is a property in the ControllerBase class

		auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
		auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
		auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
		auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
		auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

		var result = await _context.SaveChangesAsync() > 0;

		var auctionUpdated = _mapper.Map<AuctionUpdated>(updateAuctionDto);
		auctionUpdated.Id = id.ToString();

		await _publishEndpoint.Publish(auctionUpdated);

		if(result) 
		{ 
			return Ok(); 
		}

		return BadRequest("Problem saving changes");
	}

	[Authorize]
	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAuction(Guid id)
	{
		var auction = await _context.Auctions.FindAsync(id);

		if(auction == null)
		{
			return NotFound();
		}

		if (auction.Seller != User.Identity.Name) return Forbid();  // User is a property in the ControllerBase class

        _context.Auctions.Remove(auction);

		var result = await _context.SaveChangesAsync() > 0;

		var auctionDeleted = new AuctionDeleted() { Id = id.ToString() };

		await _publishEndpoint.Publish(auctionDeleted);

		if(!result) 
		{
			return BadRequest("Could not update DB");
		}

		return Ok();
	}
}
