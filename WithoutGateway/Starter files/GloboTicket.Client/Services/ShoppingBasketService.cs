using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models.Api;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Web.Services
{
    public class ShoppingBasketService : IShoppingBasketService
    {
        private readonly HttpClient client;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ShoppingBasketService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            this.client = client;
            this.httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetTokenAsync()
            => await httpContextAccessor.HttpContext.GetTokenAsync("access_token");


        public async Task<BasketLine> AddToBasket(Guid basketId, BasketLineForCreation basketLine)
        {
            if (basketId == Guid.Empty)
            {
                client.SetBearerToken(await GetTokenAsync());
                var basketResponse = await client.PostAsJson("/api/baskets", new BasketForCreation
                {
                    UserId = Guid.Parse(
                httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c =>
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value)
                });
                var basket = await basketResponse.ReadContentAs<Basket>();
                basketId = basket.BasketId;
            }

            client.SetBearerToken(await GetTokenAsync());
            var response = await client.PostAsJson($"api/baskets/{basketId}/basketlines", basketLine);
            return await response.ReadContentAs<BasketLine>();
        }

        public async Task<Basket> GetBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return null;
            client.SetBearerToken(await GetTokenAsync());
            var response = await client.GetAsync($"/api/baskets/{basketId}");
            return await response.ReadContentAs<Basket>();
        }

        public async Task<IEnumerable<BasketLine>> GetLinesForBasket(Guid basketId)
        {
            if (basketId == Guid.Empty)
                return new BasketLine[0];
            client.SetBearerToken(await GetTokenAsync());
            var response = await client.GetAsync($"/api/baskets/{basketId}/basketLines");
            return await response.ReadContentAs<BasketLine[]>();

        }

        public async Task UpdateLine(Guid basketId, BasketLineForUpdate basketLineForUpdate)
        {
            client.SetBearerToken(await GetTokenAsync());
            await client.PutAsJson($"/api/baskets/{basketId}/basketLines/{basketLineForUpdate.LineId}", basketLineForUpdate);
        }

        public async Task RemoveLine(Guid basketId, Guid lineId)
        {
            client.SetBearerToken(await GetTokenAsync());
            await client.DeleteAsync($"/api/baskets/{basketId}/basketLines/{lineId}");
        }

        public async Task<BasketForCheckout> Checkout(Guid basketId, BasketForCheckout basketForCheckout)
        {
            client.SetBearerToken(await GetTokenAsync());
            var response = await client.PostAsJson($"api/baskets/checkout", basketForCheckout);

            return response.IsSuccessStatusCode
                ? await response.ReadContentAs<BasketForCheckout>()
                : throw new Exception("Something went wrong placing your order. Please try again.");
        }
    }
}
