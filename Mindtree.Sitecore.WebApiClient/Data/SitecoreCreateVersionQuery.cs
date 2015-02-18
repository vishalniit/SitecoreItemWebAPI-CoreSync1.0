using System.Collections.Generic;
using Mindtree.Sitecore.WebApi.Client.Interfaces;
using Mindtree.Sitecore.WebApi.Client.Util;

namespace Mindtree.Sitecore.WebApi.Client.Data
{
    /// <summary>
    /// Represents a query that creates new items
    /// </summary>
    public class SitecoreCreateVersionQuery : SitecoreQuery, ISitecoreCreateQuery
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SitecoreCreateQuery" /> class.
        /// </summary>
        public SitecoreCreateVersionQuery()
            : base(SitecoreQueryType.CreateVersion)
        {

        }

        #region Overrides of SitecoreQuery

        /// <summary>
        /// Gets the query string parameter.
        /// </summary>
        /// <value>
        /// The query string parameter.
        /// </value>
        public override KeyValuePair<string, string> QueryParameter
        {
            get { return Validate() ? new KeyValuePair<string, string>(Structs.QueryStringKeys.Query, string.Empty) : new KeyValuePair<string, string>(Structs.QueryStringKeys.Query, string.Empty); }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Validate()
        {
            bool result = false;
            var valid = !string.IsNullOrWhiteSpace(Items) &&
                        QueryType == SitecoreQueryType.CreateVersion;
            if (!valid)
                result = false;
            // either an item id or query must be provided
            if (!string.IsNullOrWhiteSpace(ItemId))
            {
                valid = ItemId.IsSitecoreId();
            }
            if (valid)
                result = true;
            return result;
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public override string ErrorMessage
        {
            get { return "You must at least provide items id"; }
        }

        #endregion

        #region Implementation of ItemsID's

        /// <summary>
        /// Gets or sets the items id for which you want to create versions
        /// </summary>
        /// <value>
        /// Pipe separated Item IDs		
        /// </value>
        public string Items { get; set; }

        #endregion

        /// <summary>
        /// Gets the query string parameters.
        /// </summary>
        /// <value>
        /// The query string parameters.
        /// </value>
        public override IDictionary<string, string> QueryStringParameters
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                                     {                                         
                                         { Structs.QueryStringKeys.items, Items },
                                         { Structs.QueryStringKeys.ItemId, ItemId },
                                         { Structs.QueryStringKeys.Database, Database },
                                         { Structs.QueryStringKeys.Payload, Payload.ToString() },
                                         {Structs.QueryStringKeys.Language,Language}
                                     };

                if (FieldsToReturn != null)
                {
                    dictionary.Add(Structs.QueryStringKeys.Fields, string.Join("|", FieldsToReturn));
                }
                return dictionary;
            }
        }

        #region Implementation of ISitecoreItemQuery

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public string ItemId { get; set; }

        #endregion


        public string ParentQuery
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Template
        {
            get;
            set;
        }
    }
}
