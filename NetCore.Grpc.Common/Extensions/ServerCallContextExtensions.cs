using System;
using System.Linq;
using Grpc.Core;
using NetCore.Grpc.Common.Helpers;

namespace NetCore.Grpc.Common.Extensions
{
    public static class ServerCallContextExtensions
    {
        public static string GetLogin(this ServerCallContext context)
        {
            var loginEntry = GetEntry(context, GrpcHelper.HeaderKey.Login);
            return loginEntry?.Value;
        }

        public static string GetToken(this ServerCallContext context)
        {
            var loginEntry = GetEntry(context, GrpcHelper.HeaderKey.Token);
            return loginEntry.Value;
        }

        private static Metadata.Entry GetEntry(ServerCallContext context, string key)
        {
            return context.RequestHeaders.SingleOrDefault(x =>
                x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}