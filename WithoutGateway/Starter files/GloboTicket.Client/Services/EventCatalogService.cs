using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models.Api;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Web.Services
{
    public class EventCatalogService : IEventCatalogService
    {
        private readonly HttpClient client;
        private string token;
        public EventCatalogService(HttpClient client)
        {
            this.client = client;
        }

        private async Task<string> GetToken()
        {
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }

            var discoveryDocumentResponse = await client
                .GetDiscoveryDocumentAsync("https://localhost:5010/");

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientId = "globalticketm2m",
                ClientSecret = "0d0da600-9a2e-4a2d-932a-796ec72b0100",
                Scope = "globalticket.fullaccess"
            });

            token = tokenResponse.AccessToken;
            return token;
        }

        public async Task<IEnumerable<Event>> GetAll()
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync("/api/events");
            return await response.ReadContentAs<List<Event>>();
        }

        public async Task<IEnumerable<Event>> GetByCategoryId(Guid categoryid)
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync($"/api/events/?categoryId={categoryid}");
            return await response.ReadContentAs<List<Event>>();
        }

        public async Task<Event> GetEvent(Guid id)
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync($"/api/events/{id}");
            return await response.ReadContentAs<Event>();
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            client.SetBearerToken(await GetToken());
            var response = await client.GetAsync("/api/categories");
            return await response.ReadContentAs<List<Category>>();
        }

    }
}
