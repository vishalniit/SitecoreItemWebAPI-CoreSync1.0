﻿using System;
using System.Security.Cryptography;
using System.Text;
using Mindtree.Sitecore.WebApi.Client.Diagnostics;
using Mindtree.Sitecore.WebApi.Client.Interfaces;

namespace Mindtree.Sitecore.WebApi.Client.Util
{
    /// <summary>
    /// Security focused utility methods
    /// </summary>
    public static class SecurityUtil
    {
        /// <summary>
        /// Encrypts the header value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string EncryptHeaderValue(string value, ISitecorePublicKeyResponse key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException("value", "value cannot be null or empty when encrypting headers");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key", "key cannot be null when encrypting headers");
            }

            try
            {
                byte[] encrypted;

                using (var rsa = new RSACryptoServiceProvider())
                {
                    var rsaKeyInfo = new RSAParameters
                                         {
                                             // set rsaKeyInfo to the public key values. 
                                             Modulus = Encoding.UTF8.GetBytes(key.Modulus),
                                             Exponent = Encoding.UTF8.GetBytes(key.Exponent)
                                         };

                    rsa.ImportParameters(rsaKeyInfo);

                    encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(value), false);
                }

                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                Log.WriteError("Error encrypting header value", ex);
            }

            return string.Empty;
        }
    }
}
