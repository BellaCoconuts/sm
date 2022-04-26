using IdentityModel;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Services.ShoppingBasket.Helpers
{
    public class TokenExchangeService
    {
        private const string AccessTokenKey = "gatewaytodownstreamtokenexchangeclient_";
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IClientAccessTokenCache clientAccessTokenCache;

        public TokenExchangeService(IHttpClientFactory httpClientFactory, IClientAccessTokenCache clientAccessTokenCache)
        {
            this.httpClientFactory = httpClientFactory;
            this.clientAccessTokenCache = clientAccessTokenCache;
        }

        public async Task<string> GetTokenAsync(string incomingToken, string apiScope)
        {
            var item = await clientAccessTokenCache
                .GetAsync($"{AccessTokenKey}{apiScope}");

            if (item != null)
            {
                return item.AccessToken;
            }

            (string accessToken, int expiresIn) = await ExchangeToken(incomingToken, apiScope);

            await clientAccessTokenCache.SetAsync($"{AccessTokenKey}{apiScope}", accessToken, expiresIn);

            return accessToken;
        }

        private async Task<(string, int)> ExchangeToken(string incomingToken, string apiScope)
        {
            var client = httpClientFactory.CreateClient();

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync("https://localhost:5010/");

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var scopes = $"{OidcConstants.StandardScopes.OpenId} {OidcConstants.StandardScopes.Profile} {apiScope}";

            var customParams = new Dictionary<string, string>()
            {
                { OidcConstants.TokenRequest.SubjectTokenType, OidcConstants.TokenTypeIdentifiers.AccessToken },
                { OidcConstants.TokenRequest.SubjectToken, incomingToken},
                { OidcConstants.TokenRequest.Scope, scopes }
            };

            var tokenResponse = await client.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                GrantType = OidcConstants.GrantTypes.TokenExchange,
                Parameters = customParams,
                ClientId = "gatewaytodownstreamtokenexchangeclient",
                ClientSecret = "ce766e16-df99-411d-8d31-0f5bbc6b8ebc"
            });

            return !tokenResponse.IsError ? (tokenResponse.AccessToken, tokenResponse.ExpiresIn) : throw new Exception(tokenResponse.Error);
        }
    }
}
