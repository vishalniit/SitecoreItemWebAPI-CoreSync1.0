using System;
using System.IO;
using System.Net;

namespace Mindtree.Sitecore.WebApi.Client.Interfaces
{
    /// <summary>
    /// Defines the properties and methods that objects wishing to send authenticated requests to the Sitecore Item Web API must implement
    /// </summary>
    public interface IAuthenticatedSitecoreDataContext : ISitecoreDataContext
    {
        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>
        /// The credentials.
        /// </value>
        ISitecoreCredentials Credentials { get; }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <returns></returns>
        HttpWebRequest CreateRequest(Uri uri, SitecoreQueryType type, byte[] formData, string contentType);

        /// <summary>
        /// Applies the headers.
        /// </summary>
        /// <param name="request">The request.</param>
        void ApplyHeaders(HttpWebRequest request);

        /// <summary>
        /// Applies the encrypted headers.
        /// </summary>
        /// <param name="request">The request.</param>
        void ApplyEncryptedHeaders(HttpWebRequest request);
    }
}
