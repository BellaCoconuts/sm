using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalTicket.Services.Identity.Services
{
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        public string GrantType => "urn:ieft:params:oauth:grant-type:token-exchange";
        private const string accessTokenType = "urn:ieft:params:oauth:token-type:access_token";
        private readonly ITokenValidator validator;

        public TokenExchangeExtensionGrantValidator(ITokenValidator validator)
        {
            this.validator = validator;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var requestedGrant = context.Request.Raw.Get("grant_type");
            if (string.IsNullOrWhiteSpace(requestedGrant) || requestedGrant != GrantType)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid grant.");
                return;
            }

            var subjectToken = context.Request.Raw.Get("subject_token");
            if (string.IsNullOrWhiteSpace(subjectToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token missing.");
                return;
            }

            var subjectTokenType = context.Request.Raw.Get("subject_token_type");
            if (string.IsNullOrWhiteSpace(subjectTokenType))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token type missing.");
                return;
            }

            if (subjectTokenType != accessTokenType)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token type invalid.");
                return;
            }

            var result = await validator.ValidateAccessTokenAsync(subjectToken);
            if (result.IsError)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Subject token invalid.");
                return;
            }

            var subjectClaim = result.Claims.FirstOrDefault(c => c.Type == "sub");
            if (subjectClaim is null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token must contain sub value.");
                return;
            }

            // Potential authorization checks.

            // Potential claims transformations.

            context.Result = new GrantValidationResult(subjectClaim.Value, "access_token", result.Claims);
            return;
        }
    }
}
