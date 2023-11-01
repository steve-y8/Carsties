using Duende.IdentityServer.Models;

namespace IdentityService
{
	public static class Config
	{
		public static IEnumerable<IdentityResource> IdentityResources =>
			new IdentityResource[]
			{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
			};

		public static IEnumerable<ApiScope> ApiScopes =>
			new ApiScope[]
			{
				new ApiScope("auctionApp", "Auction app full access")
			};

		public static IEnumerable<Client> Clients =>
			new Client[]
			{
				new Client
				{
					ClientId = "postman",
					ClientName = "Postman",
					AllowedScopes = 
					{
						"openid",		// Matched with the IdentityResources above
						"profile",		// Matched with the IdentityResources above
						"auctionApp"	// Matched with the ApiScopes above
					},
					RedirectUris = {"https://www.getpostman.com/oauth2/callback"},	// Not going to redirect Postman to anywhere
					ClientSecrets = new []{new Secret("NotASecret".Sha256())},
					AllowedGrantTypes = {GrantType.ResourceOwnerPassword}	// Use user password to get token from IdentityServer
																			// ResourceOwnerPassword grant type will be deprecated in OAuth 2.1
				}
			};
	}
}