using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NetCore.Grpc.Server
{
    /// <summary>
    /// Authentication options
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// Issuer of token
        /// </summary>
        public const string Issuer = "MyAuthServer"; 
        
        /// <summary>
        /// Audience of token
        /// </summary>
        public const string Audience = "http://localhost:5000/";
        
        /// <summary>
        /// Hash key
        /// </summary>
        private const string Key = "mysupersecret_secretkey!123";
        
        /// <summary>
        /// Token life time
        /// </summary>
        public const int Lifetime = 40;
        
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}