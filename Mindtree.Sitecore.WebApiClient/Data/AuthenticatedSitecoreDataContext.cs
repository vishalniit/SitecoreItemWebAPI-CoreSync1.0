using System;
using System.Net;
using System.Text;
using Mindtree.Sitecore.WebApi.Client.Diagnostics;
using Mindtree.Sitecore.WebApi.Client.Interfaces;
using Mindtree.Sitecore.WebApi.Client.Net;
using Mindtree.Sitecore.WebApi.Client.Util;
using System.IO;
using System.Collections.Generic;
using Mindtree.Sitecore.WebApi.Client.Serialization;


namespace Mindtree.Sitecore.WebApi.Client.Data
{
    /// <summary>
    /// Represents an authenticated Sitecore data context
    /// </summary>
    public class AuthenticatedSitecoreDataContext : SitecoreDataContext, IAuthenticatedSitecoreDataContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedSitecoreDataContext" /> class.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="isSecure">if set to <c>true</c> [is secure].</param>
        /// <param name="credentials">The credentials.</param>
        /// <exception cref="System.ArgumentException">credentials</exception>
        public AuthenticatedSitecoreDataContext(string hostName, ISitecoreCredentials credentials, bool isSecure = false)
            : base(hostName, isSecure)
        {
            if (isSecure && credentials.EncryptHeaders)
            {
                throw new InvalidOperationException("If you use an SSL connection, the credentials must not be encrypted. The server takes care of header encryption.");
            }

            if (credentials == null)
            {
                throw new ArgumentNullException("credentials", "credentials cannot be null when creating a new instance of AuthenticatedSitecoreDataContext");
            }

            if (!credentials.Validate())
            {
                throw new ArgumentException(credentials.ErrorMessage, "credentials");
            }

            Credentials = credentials;
        }

        #region Implementation of IAuthenticatedSitecoreDataContext

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>
        /// The credentials.
        /// </value>
        public ISitecoreCredentials Credentials { get; private set; }

        /// <summary>
        /// Applies the headers.
        /// </summary>
        /// <param name="request">The request.</param>
        public void ApplyHeaders(HttpWebRequest request)
        {
            if (request == null)
                return;

            if (Credentials.EncryptHeaders)
            {
                ApplyEncryptedHeaders(request);
                return;
            }

            request.Headers.Add(Structs.AuthenticationHeaders.UserName, Credentials.UserName);
            request.Headers.Add(Structs.AuthenticationHeaders.Password, Credentials.Password);
        }

        /// <summary>
        /// Applies the encrypted headers.
        /// </summary>
        /// <param name="request">The request.</param>
        public void ApplyEncryptedHeaders(HttpWebRequest request)
        {
            if (request == null)
                return;

            var key = GetPublicKey();

            if (key != null)
            {
                request.Headers.Add(Structs.AuthenticationHeaders.UserName,
                                    SecurityUtil.EncryptHeaderValue(Credentials.UserName, key));
                request.Headers.Add(Structs.AuthenticationHeaders.Password,
                                    SecurityUtil.EncryptHeaderValue(Credentials.Password, key));
                request.Headers.Add(Structs.AuthenticationHeaders.Encrypted, "1");
            }
            else
            {
                Log.WriteWarn("Could not retrieve a public key to send encypted headers, authentication headers were not passed with the request");
            }
        }

        string contentType;
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <returns></returns>
        public virtual HttpWebRequest CreateRequest(Uri uri, SitecoreQueryType type, byte[] formData, string contentType)
        {
            var request = CreateRequest(uri, type);
            try
            {
                request.KeepAlive = true;
                request.ContentLength = formData.Length;
                request.ContentType = contentType;
                var requestStream = request.GetRequestStream();
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            catch (Exception ex)
            {
                Log.WriteError("Could not add post data to the HttpWebRequest", ex);
            }
            return request;
        }

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <returns></returns>
        public virtual HttpWebRequest CreateRequest(Uri uri, SitecoreQueryType type, string postData, string contentType)
        {
            var request = CreateRequest(uri, type);
            request.ContentType = contentType;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(postData);

                request.ContentLength = buffer.Length;

                var requestStream = request.GetRequestStream();

                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
            }
            catch (Exception ex)
            {
                //LogFactory.Error("Could not add post data to the HttpWebRequest", ex);
            }

            return request;
        }
        #endregion

        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="type"></param>
        /// <param name="fsMediaItem"></param>
        /// <returns></returns>
        public override HttpWebRequest CreateRequest(Uri uri, SitecoreQueryType type)
        {
            var request = base.CreateRequest(uri, type);
            ApplyHeaders(request);
            return request;
        }

        private Dictionary<string, object> FillMedia(IBaseQuery query)
        {
            Dictionary<string, object> postParameters = null;
            if (query != null)
            {
                FileStream fs; byte[] filedata = null;
                if (query.QueryType == SitecoreQueryType.AdvanceCreate)
                {
                    fs = ((ISitecoreAdvanceQuery)query).MediaItemStream;
                }
                else
                {
                    fs = ((ISitecoreCreateQuery)query).MediaItemStream;
                }
                if (fs != null)
                {
                    filedata = new byte[fs.Length];
                    fs.Read(filedata, 0, filedata.Length);
                    fs.Close();
                    if (filedata != null)
                    {
                        postParameters = new Dictionary<string, object>();
                        postParameters.Add(Mindtree.ItemWebApi.Pipelines.Configuration.Settings.file, new FileParameter(filedata, fs.Name, ""));
                    }
                }
            }
            return postParameters;
        }
        /// <summary>
        /// Gets the response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">query</exception>
        public override T GetResponse<T>(IBaseQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            // build the query
            var uri = query.BuildUri(HostName);
            HttpWebRequest request = null; T response = null;
            contentType = string.Empty;
            string formDataBoundary = string.Empty;
            Dictionary<string, object> postParameters = null;
            byte[] formData = null;
            formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            contentType = Mindtree.ItemWebApi.Pipelines.Configuration.Settings.ContentTypeMultipleFormData + " boundary=" + formDataBoundary;
            try
            {
                switch (query.QueryType)
                {
                    case SitecoreQueryType.AdvanceCreate:
                        string loadOptions = ((ISitecoreAdvanceQuery)query).loadOptions;
                        string syncItem = ((ISitecoreAdvanceQuery)query).syncItem;
                        string encryptionKey = ((ISitecoreAdvanceQuery)query).EncryptionKey;
                        // Generate post objects
                        postParameters = FillMedia(query);
                        if (postParameters == null)
                            postParameters = new Dictionary<string, object>();
                        postParameters.Add(Mindtree.ItemWebApi.Pipelines.Configuration.Settings.loadOptions, loadOptions);
                        postParameters.Add(Mindtree.ItemWebApi.Pipelines.Configuration.Settings.syncItem, syncItem);
                        postParameters.Add(Mindtree.ItemWebApi.Pipelines.Configuration.Settings.enKey, encryptionKey);
                        formData = FormUpload.GetMultipartFormDataOne(postParameters, formDataBoundary);
                        request = CreateRequest(uri, query.QueryType, formData, contentType);
                        break;
                    case SitecoreQueryType.Create:
                    case SitecoreQueryType.Update:
                    case SitecoreQueryType.CreateVersion:
                        postParameters = FillMedia(query);
                        if (postParameters == null)
                            postParameters = new Dictionary<string, object>();
                        foreach (var item in ((ISitecoreQuery)query).FieldsToUpdate)
                        {
                            postParameters.Add(item.Key, item.Value);
                        }
                        formData = FormUpload.GetMultipartFormData(postParameters, formDataBoundary);
                        request = CreateRequest(uri, query.QueryType, formData, contentType);
                        break;
                    default:
                        //FileStream fs = null;
                        request = CreateRequest(uri, query.QueryType);
                        request.ContentType = Mindtree.ItemWebApi.Pipelines.Configuration.Settings.ContentTypeFormData;
                        break;
                }
                // send the request                        
                if (request != null)
                    response = Get(request, query.ResponseFormat, new T());
            }
            catch (Exception ex)
            { Log.WriteError(ex.Message, ex); }
            finally { formData = null; request = null; formDataBoundary = null; contentType = null; }
            // return the response
            return response;
        }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <returns></returns>
        public override ISitecorePublicKeyResponse GetPublicKey()
        {
            var query = new SitecoreActionQuery("getpublickey");

            // do not authenticate the call to get public key otherwise you will end up in an eternal loop
            // as the authentication routine itself calls GetPublicKey()

            ISitecorePublicKeyResponse response = new SitecoreDataContext(HostName).GetResponse<SitecorePublicKeyResponse>(query);

            return response.Validate() ? response : null;
        }
    }
}
