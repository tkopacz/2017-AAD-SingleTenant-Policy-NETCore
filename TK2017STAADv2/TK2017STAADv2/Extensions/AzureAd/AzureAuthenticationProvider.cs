using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Microsoft.AspNetCore.Authentication.Extensions
{
    internal class AzureAuthenticationProvider : IAuthenticationProvider
    {
        private AzureAdOptions m_aadOptions;
        private ClaimsPrincipal m_principal;
        private string m_code;

        public AzureAuthenticationProvider(AzureAdOptions aadOptions, ClaimsPrincipal principal, string code)
        {
            m_aadOptions = aadOptions;
            this.m_principal = principal;
            this.m_code = code;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string signedInUserID = m_principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            //This will work for multitenant apps
            string tenantID = m_principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            //This will work only for single tenant app
            //string tenantID = m_aadOptions.TenantId;
            var authContext = new AuthenticationContext($"{m_aadOptions.AzureAdSingleInstance}{tenantID}");
            var creds = new ClientCredential(m_aadOptions.ClientId, m_aadOptions.ClientSecret);
            var redirectUri = new Uri($"{m_aadOptions.CallbackDomain}{m_aadOptions.CallbackPath}");
            var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                m_code, redirectUri, creds,
                "https://graph.microsoft.com/");

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}