﻿using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Web.Services
{
    public class EventCatalogService : IEventCatalogService
    {
        private readonly HttpClient client;

        public EventCatalogService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Event>> GetAll()
        {
            // client.SetBearerToken(await httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync("api/events");
            return await response.ReadContentAs<List<Event>>();
        }

        public async Task<IEnumerable<Event>> GetByCategoryId(Guid categoryid)
        {
            // client.SetBearerToken(await httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync($"api/events/?categoryId={categoryid}");
            return await response.ReadContentAs<List<Event>>();
        }

        public async Task<Event> GetEvent(Guid id)
        {
            // client.SetBearerToken(await httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync($"api/events/{id}");
            return await response.ReadContentAs<Event>();
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            // client.SetBearerToken(await httpContextAccessor.HttpContext.GetTokenAsync("access_token"));
            var response = await client.GetAsync("api/categories");
            return await response.ReadContentAs<List<Category>>();
        }

    }
}
