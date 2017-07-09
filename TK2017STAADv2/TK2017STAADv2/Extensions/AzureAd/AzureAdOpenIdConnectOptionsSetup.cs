using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Graph;

namespace Microsoft.AspNetCore.Authentication.Extensions
{
    public class AzureAdOpenIdConnectOptionsSetup : IConfigureOptions<OpenIdConnectOptions>
    {
        private readonly AzureAdOptions _aadOptions;

        public AzureAdOpenIdConnectOptionsSetup(IOptions<AzureAdOptions> aadOptions)
        {
            _aadOptions = aadOptions.Value;
        }

        public void Configure(OpenIdConnectOptions oidcOptions)
        {
            oidcOptions.ClientId = _aadOptions.ClientId;
            oidcOptions.Authority = _aadOptions.Authority;
            oidcOptions.UseTokenLifetime = true;
            oidcOptions.CallbackPath = _aadOptions.CallbackPath;
            oidcOptions.ClientSecret = _aadOptions.ClientSecret;

            //We need id_token (login) and code (to call Graph API / another Web Api)
            oidcOptions.ResponseType = "code id_token";

            oidcOptions.Events = new OpenIdConnectEvents
            {
                OnTicketReceived = (context) =>
                {
                    // If your authentication logic is based on users then add your logic here
                    return Task.FromResult(0);
                },
                OnAuthenticationFailed = (context) =>
                {
                    context.Response.Redirect("/Home/Error");
                    context.HandleResponse(); // Suppress the exception
                    return Task.FromResult(0);
                },
                // If your application needs to do authenticate single users, add your user validation below.
                OnTokenValidated = (context) =>
                {
                    return myUserValidationLogic(context.Ticket.Principal);
                },
                OnAuthorizationCodeReceived = (context) =>
                {
                    Task.Run(async () =>
                    {
                        Debug.WriteLine(context.TokenEndpointRequest.Code);
                        try
                        {
                            List<Claim> claims = new List<Claim>();
                            var gsc = new GraphServiceClient(new AzureAuthenticationProvider(_aadOptions, context.Ticket.Principal, context.TokenEndpointRequest.Code));

                            var me = await gsc.Me.Request().GetAsync();
                            if (me.JobTitle == "jobtitle")
                            {
                                //Add any claim based on Graph API - 
                                claims.Add(new Claim("tkclaim", "ok"));
                            }

                            //Update principal
                            var principal = context.Ticket.Principal;
                            //Add Claims
                            (principal.Identity as ClaimsIdentity).AddClaims(claims);
                            //Replace ticket
                            context.Ticket = new AuthenticationTicket(
                                                 principal,
                                                 context.Ticket.Properties,
                                                 context.Ticket.AuthenticationScheme);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }).Wait();

                    return Task.FromResult(0);
                }
            };
        }

        private Task myUserValidationLogic(ClaimsPrincipal principal)
        {
            //Or check in DB or...
            if (principal.Identity.Name == "ABC") throw new UnauthorizedAccessException();
            return Task.FromResult(0);
        }
    }
}

