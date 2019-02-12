using System;
using System.Security.Cryptography;
using System.Text;

namespace Aims.EndpointAgent.Configuration
{
    public static class StringExtensions
    {
        public static string Unprotect(this string protectedString)
        {
            return Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                    Convert.FromBase64String(protectedString), 
                    null, DataProtectionScope.CurrentUser));
        }
    }
}