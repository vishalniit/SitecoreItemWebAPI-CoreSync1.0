using Mindtree.Sitecore.WebApi.Client.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Mindtree.Sitecore.WebApi.Client.Interfaces
{
    /// <summary>
    /// Defines the properties and methods that objects wishing to send queries to the Sitecore Item Web API must implement
    /// </summary>
    public interface ISitecoreAdvanceQuery : IBaseQuery
    {
        /// <summary>
        /// Get or sets the loadOption Object
        /// Serializer is XML
        /// <value>
        /// Sitecore.Data.Serialization.LoadOptions object serialized in ItemQuery
        /// </value>
        /// </summary>
        string loadOptions { get; set; }

        /// <summary>
        /// Get or sets the SyncItem Object
        /// Serializer is Custom Sitecore Text
        /// <value>
        /// Sitecore.Data.Serialization.ObjectModel object serialized in ItemQuery
        /// </value>
        /// </summary>
        string syncItem { get; set; }

        /// <summary>
        /// Get or sets the string value to decrypt the passed value
        /// Serializer is Custom Sitecore Text
        /// <value>
        /// string Decryption key
        /// </value>
        /// </summary>
        string EncryptionKey { get; set; }

        /// <summary>
        /// Get or sets the media item for 
        /// </summary>
        /// <value>
        /// File stream 
        /// </value>
        FileStream MediaItemStream { get; set; }
    }
}
