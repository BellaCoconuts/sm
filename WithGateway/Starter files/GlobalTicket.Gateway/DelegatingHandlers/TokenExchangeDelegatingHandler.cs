using GlobalTicket.Core;
using IdentityModel;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalTicket.Gateway.DelegatingHandlers
{
    public class TokenExchangeDelegatingHandler : DelegatingHandler
    {
        private const string AccessTokenKey = "gatewaytodownstreamtokenexchangeclient_eventcatalog";
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IClientAccessTokenCache clientAccessTokenCache;

        public TokenExchangeDelegatingHandler(IHttpClientFactory httpClientFactory, IClientAccessTokenCache clientAccessTokenCache)
        {
            this.httpClientFactory = httpClientFactory;
            this.clientAccessTokenCache = clientAccessTokenCache;
        }

        private async Task<string> GetAccessToken(string incomingToken)
        {
            var item = await clientAccessTokenCache
                .GetAsync(AccessTokenKey);

            if (item != null)
            {
                return item.AccessToken;
            }

            (string accessToken, int expiresIn) = await ExchangeToken(incomingToken);

            await clientAccessTokenCache.SetAsync(AccessTokenKey, accessToken, expiresIn);

            return accessToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // extract current token
            var incomingToken = request.Headers.Authorization.Parameter;

            // exchange token
            var accessToken = await GetAccessToken(incomingToken);

            // set bearer token
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<(string, int)> ExchangeToken(string token)
        {
            var client = httpClientFactory.CreateClient();

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync("https://localhost:5010/");

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var scopes = $"{OidcConstants.StandardScopes.OpenId} {OidcConstants.StandardScopes.Profile} {ScopeConstants.EventCatalog.FullAccess}";

            var customParams = new Dictionary<string, string>()
            {
                { OidcConstants.TokenRequest.SubjectTokenType, OidcConstants.TokenTypeIdentifiers.AccessToken },
                { OidcConstants.TokenRequest.SubjectToken, token},
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



