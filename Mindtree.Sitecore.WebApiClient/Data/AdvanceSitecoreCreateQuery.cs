using System.Collections.Generic;
using Mindtree.Sitecore.WebApi.Client.Interfaces;
using Mindtree.Sitecore.WebApi.Client.Util;
using System;
using Mindtree.Sitecore.WebApi.Client.Diagnostics;
using Mindtree.Sitecore.WebApi.Client.Serialization;
using System.IO;

namespace Mindtree.Sitecore.WebApi.Client.Data
{
    /// <summary>
    /// Represents a query that creates new items
    /// </summary>
    public class SitecoreAdvanceCreateQuery : ISitecoreAdvanceQuery
    {
        private int _apiVersion;

        private string _database;

        private int _itemVersion;

        public SitecoreAdvanceCreateQuery(SitecoreQueryType type, ResponseFormat format = ResponseFormat.Json)
        {
            QueryType = type;
            ResponseFormat = format;
        }

        /// <summary>
        /// Get or sets the loadOption Object
        /// Serializer is XML
        /// <value>
        /// Sitecore.Data.Serialization.LoadOptions object serialized in ItemQuery
        /// </value>
        /// </summary>
        public string loadOptions
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets the string value to decrypt the passed value
        /// Serializer is Custom Sitecore Text
        /// <value>
        /// string Decryption key
        /// </value>
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        /// <value>
        /// The API version.
        /// </value>
        public int ApiVersion
        {
            get { return _apiVersion > 0 ? _apiVersion : SettingsUtility.DefaultApiVersion; }
            set { _apiVersion = value; }
        }

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database
        {
            get { return _database ?? SettingsUtility.DefaultDatabase; }
            set { _database = value; }
        }

        /// <summary>
        /// Gets or sets the item version.
        /// </summary>
        /// <value>
        /// The item version.
        /// </value>
        public int ItemVersion
        {
            get { return _itemVersion > 0 ? _itemVersion : 1; }
            set { _itemVersion = value; }
        }

        /// <summary>
        /// mandatory to pass the true value
        /// as without this WebAPI will not identify
        /// it as a advance create request
        /// </summary>
        public bool RetainID { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<string> FieldsToReturn { get; set; }

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public string ItemId { get; set; }

        public IDictionary<string, string> QueryStringParameters
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                                     {
                                         { Structs.QueryStringKeys.RetainID, RetainID.ToString() },
                                         { Structs.QueryStringKeys.Database, Database },                                        
                                     };

                if (FieldsToReturn != null)
                {
                    dictionary.Add(Structs.QueryStringKeys.Fields, string.Join("|", FieldsToReturn));
                }

                if (!string.IsNullOrWhiteSpace(ItemId))
                {
                    dictionary.Add(Structs.QueryStringKeys.ItemId, ItemId);
                }                

                return dictionary;
            }
        }

        public SitecoreQueryType QueryType
        {
            get;
            set;
        }

        public System.Uri BuildUri(string hostName)
        {
            if (!Validate())
            {
                throw new InvalidOperationException("You cannot build a web service URI with an invalid data query");
            }

            try
            {
                var uriSuffix = string.Format("{0}/-/item/v{1}/?", hostName.TrimEnd('/'), ApiVersion);

                var uri = string.Format("{0}{1}", uriSuffix, QueryStringParameters.ToQueryString());

                return new Uri(uri);
            }
            catch (Exception ex)
            {
                Log.WriteError("Could not build a URI for the data query", ex);
            }

            return null;
        }

        public ResponseFormat ResponseFormat
        {
            get;
            set;
        }

        public bool Validate()
        {
            var valid = RetainID &&
                        QueryType == SitecoreQueryType.AdvanceCreate;
            if (!valid)
                return false;
            if (loadOptions != null && syncItem != null)
            {
                valid = true;
            }

            return valid;
        }

        public string ErrorMessage
        {
            get { return "You must provide the loadoptions and syncItem and set RetainID as true"; }
        }

        /// <summary>
        /// Get or sets the SyncItem Object
        /// Serializer is Custom Sitecore Text
        /// <value>
        /// Sitecore.Data.Serialization.ObjectModel object serialized in ItemQuery
        /// </value>
        /// </summary>
        public string syncItem
        {
            get;
            set;
        }

        public FileStream MediaItemStream { get; set; }
    }
}
