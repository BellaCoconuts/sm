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
                new ApiResource("globalticket", "GlobalTicket API")
                {
                    Scopes = new[] { "globalticket.fullaccess" }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("globalticket.fullaccess")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client()
                {
                    ClientName = "GlobalTicket Machine 2 Machine Client",
                    ClientId = "globalticketm2m",
                    ClientSecrets = { new Secret("0d0da600-9a2e-4a2d-932a-796ec72b0100".Sha256())},
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "globalticket.fullaccess" }
                }
            };
    }
}