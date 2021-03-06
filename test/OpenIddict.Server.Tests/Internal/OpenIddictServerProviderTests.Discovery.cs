/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Client;
using AspNet.Security.OpenIdConnect.Primitives;
using Newtonsoft.Json.Linq;
using Xunit;

namespace OpenIddict.Server.Internal.Tests
{
    public partial class OpenIddictServerProviderTests
    {
        [Fact]
        public async Task HandleConfigurationRequest_PlainCodeChallengeMethodIsNotReturned()
        {
            // Arrange
            var server = CreateAuthorizationServer();

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.DoesNotContain(
                OpenIdConnectConstants.CodeChallengeMethods.Plain,
                ((JArray) response[OpenIdConnectConstants.Metadata.CodeChallengeMethodsSupported]).Values<string>());
        }

        [Theory]
        [InlineData(OpenIdConnectConstants.GrantTypes.AuthorizationCode)]
        [InlineData(OpenIdConnectConstants.GrantTypes.ClientCredentials)]
        [InlineData(OpenIdConnectConstants.GrantTypes.Implicit)]
        [InlineData(OpenIdConnectConstants.GrantTypes.Password)]
        [InlineData(OpenIdConnectConstants.GrantTypes.RefreshToken)]
        public async Task HandleConfigurationRequest_EnabledFlowsAreReturned(string flow)
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options =>
                {
                    options.GrantTypes.Clear();
                    options.GrantTypes.Add(flow);
                });
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);
            var types = ((JArray) response[OpenIdConnectConstants.Metadata.GrantTypesSupported]).Values<string>();

            // Assert
            Assert.Single(types);
            Assert.Contains(flow, types);
        }

        [Fact]
        public async Task HandleConfigurationRequest_NoSupportedScopesPropertyIsReturnedWhenNoScopeIsConfigured()
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options =>
                {
                    options.GrantTypes.Remove(OpenIdConnectConstants.GrantTypes.RefreshToken);
                    options.Scopes.Clear();
                });
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.False(response.HasParameter(OpenIdConnectConstants.Metadata.ScopesSupported));
        }

        [Theory]
        [InlineData(OpenIdConnectConstants.Scopes.OpenId)]
        public async Task HandleConfigurationRequest_DefaultScopesAreReturned(string scope)
        {
            // Arrange
            var server = CreateAuthorizationServer();

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.Contains(scope, ((JArray) response[OpenIdConnectConstants.Metadata.ScopesSupported]).Values<string>());
        }

        [Fact]
        public async Task HandleConfigurationRequest_CustomScopeIsReturned()
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options =>
                {
                    options.Scopes.Clear();
                    options.Scopes.Add("custom_scope");
                });
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.Contains("custom_scope", ((JArray) response[OpenIdConnectConstants.Metadata.ScopesSupported]).Values<string>());
        }

        [Fact]
        public async Task HandleConfigurationRequest_OfflineAccessScopeIsReturnedWhenRefreshTokenFlowIsEnabled()
        {
            // Arrange
            var server = CreateAuthorizationServer();

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.Contains(OpenIdConnectConstants.Scopes.OfflineAccess,
                ((JArray) response[OpenIdConnectConstants.Metadata.ScopesSupported]).Values<string>());
        }

        [Fact]
        public async Task HandleConfigurationRequest_OfflineAccessScopeIsReturnedWhenRefreshTokenFlowIsDisabled()
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options =>
                {
                    // Note: at least one flow must be enabled.
                    options.GrantTypes.Clear();
                    options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
                });
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.DoesNotContain(OpenIdConnectConstants.Scopes.OfflineAccess,
                ((JArray) response[OpenIdConnectConstants.Metadata.ScopesSupported]).Values<string>());
        }

        [Fact]
        public async Task HandleConfigurationRequest_NoSupportedClaimsPropertyIsReturnedWhenNoClaimIsConfigured()
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options => options.Claims.Clear());
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.False(response.HasParameter(OpenIdConnectConstants.Metadata.ClaimsSupported));
        }

        [Fact]
        public async Task HandleConfigurationRequest_DefaultClaimsAreReturned()
        {
            // Arrange
            var server = CreateAuthorizationServer();

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);
            var claims = ((JArray) response[OpenIdConnectConstants.Metadata.ClaimsSupported]).Values<string>().ToArray();

            // Assert
            Assert.Equal(6, claims.Length);
            Assert.Contains(OpenIdConnectConstants.Claims.Audience, claims);
            Assert.Contains(OpenIdConnectConstants.Claims.ExpiresAt, claims);
            Assert.Contains(OpenIdConnectConstants.Claims.IssuedAt, claims);
            Assert.Contains(OpenIdConnectConstants.Claims.Issuer, claims);
            Assert.Contains(OpenIdConnectConstants.Claims.JwtId, claims);
            Assert.Contains(OpenIdConnectConstants.Claims.Subject, claims);
        }

        [Fact]
        public async Task HandleConfigurationRequest_ConfiguredClaimsAreReturned()
        {
            // Arrange
            var server = CreateAuthorizationServer(builder =>
            {
                builder.Configure(options => options.Claims.Add("custom_claim"));
            });

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.Contains("custom_claim", ((JArray) response[OpenIdConnectConstants.Metadata.ClaimsSupported]).Values<string>());
        }

        [Fact]
        public async Task HandleConfigurationRequest_DefaultParametersAreReturned()
        {
            // Arrange
            var server = CreateAuthorizationServer();

            var client = new OpenIdConnectClient(server.CreateClient());

            // Act
            var response = await client.GetAsync(ConfigurationEndpoint);

            // Assert
            Assert.False((bool) response[OpenIdConnectConstants.Metadata.ClaimsParameterSupported]);
            Assert.False((bool) response[OpenIdConnectConstants.Metadata.RequestParameterSupported]);
            Assert.False((bool) response[OpenIdConnectConstants.Metadata.RequestUriParameterSupported]);
        }
    }
}
