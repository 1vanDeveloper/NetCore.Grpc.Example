using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using proto = NetCore.Grpc.Common.Protos;

namespace NetCore.Grpc.Server.Services
{
    /// <summary>
    /// Proto AuthenticationService implementation
    /// </summary>
    public class AuthenticationService: proto.AuthenticationService.AuthenticationServiceBase
    {
        private readonly Dictionary<string, string> _stubUsers = new Dictionary<string, string>
        {
            { "User1", "1" },
            { "User2", "2" }
        };
        
        public override Task<proto.AuthenticateResponse> Login(proto.AuthenticateRequest request, ServerCallContext context)
        {
            var identity = GetIdentity(request.Login, request.Password);
            if (identity == null)
            {
                return Task.FromResult(new proto.AuthenticateResponse
                {
                    Login = string.Empty,
                    Token = string.Empty
                });
            }
 
            var now = DateTime.UtcNow;
            
            // create JWT-token
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                audience: AuthOptions.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.Lifetime)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
 
            return Task.FromResult(new proto.AuthenticateResponse
            {
                Login = request.Login,
                Token = encodedJwt
            });
        }

        public override Task<proto.Response> Logout(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new proto.Response
            {
                IsSuccess = true,
                Description = string.Empty
            });
        }
        
        private ClaimsIdentity GetIdentity(string username, string password)
        {
            // todo: check user/password
            if (!_stubUsers.TryGetValue(username, out var pass) || !pass.Equals(password))
            {
                return null;
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, username),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "user")
            };
            
            var claimsIdentity = new ClaimsIdentity(
                    claims, 
                    "Token", 
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}