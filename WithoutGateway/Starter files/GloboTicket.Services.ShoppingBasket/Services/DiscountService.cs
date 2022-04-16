using GloboTicket.Services.ShoppingBasket.Extensions;
using GloboTicket.Services.ShoppingBasket.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Services.ShoppingBasket.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly HttpClient client;
        private readonly IHttpContextAccessor httpContextAccessor;
        private string accessToken;

        public DiscountService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this.client = client;
            this.httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetToken()
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync("https://localhost:5010/");
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var subjectToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            var customParams = new Dictionary<string, string>
            {
                { "subject_token_type", "urn:ieft:params:oauth:token-type:access_token" },
                { "subject_token",  subjectToken },
                { "scope", "openid profile discount.fullaccess" },
            };

            var tokenRepsonse = await client.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = "urn:ieft:params:oauth:grant-type:token-exchange",
                Parameters = customParams,
                ClientId = "shoppingbaskettodownstreamtokenexchangeclient",
                ClientSecret = "0d0da600-9a2e-4a2d-932a-796ec72b010f"
            });

            if (tokenRepsonse.IsError)
            {
                throw new Exception(tokenRepsonse.Error);
            }

            accessToken = tokenRepsonse.AccessToken;
            return accessToken;
        }

        public async Task<Coupon> GetCoupon(Guid userId)
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync($"/api/discount/user/{userId}");
            return !response.IsSuccessStatusCode ? null : await response.ReadContentAs<Coupon>();
        }
    }
}
