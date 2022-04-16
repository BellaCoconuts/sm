// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace GlobalTicket.Services.Identity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("eventcatalog", "Event catalog API")
                {
                    Scopes = new[] { "eventcatalog.read", "eventcatalog.write" }
                },
                new ApiResource("shoppingbasket", "Shopping basket API")
                {
                    Scopes = new[] { "shoppingbasket.fullaccess" }
                },
                new ApiResource("discount", "Discount API")
                {
                    Scopes = new[] { "discount.fullaccess" }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("eventcatalog.fullaccess"),
                new ApiScope("shoppingbasket.fullaccess"),
                new ApiScope("eventcatalog.read"),
                new ApiScope("eventcatalog.write"),
                new ApiScope("discount.fullaccess")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client()
                {
                    ClientName = "GlobalTicket Client",
                    ClientId = "globalticket",
                    ClientSecrets = { new Secret("0d0da600-9a2e-4a2d-932a-796ec72b010e".Sha256())},
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    RedirectUris={ "https://localhost:5000/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5000/signout-callback-oidc"},
                    AllowedScopes = { "openid", "profile", "shoppingbasket.fullaccess", "eventcatalog.read", "eventcatalog.write" }
                },
                new Client()
                {
                    ClientName = "Shopping Basket Token Exchange Client",
                    ClientId = "shoppingbaskettodownstreamtokenexchangeclient",
                    ClientSecrets = { new Secret("0d0da600-9a2e-4a2d-932a-796ec72b010f".Sha256())},
                    AllowedGrantTypes = new[] { "urn:ieft:params:oauth:grant-type:token-exchange" },
                    AllowedScopes = { "openid", "profile", "discount.fullaccess" }
                }
            };
    }
}