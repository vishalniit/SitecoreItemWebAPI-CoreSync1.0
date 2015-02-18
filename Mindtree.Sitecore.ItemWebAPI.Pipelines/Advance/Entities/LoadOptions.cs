using Sitecore.Data.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities
{
    /// <summary>
    /// Load Option Class which mimic the serializable version of
    /// actual Sitecore.Data.Serialize.LoadOption class.
    /// Difference is only Database property is in string
    /// </summary>
    [Serializable]
    public class LoadOptions
    {
        private string _database;
        private bool _forceUpdate;
        private bool _useNewId;
        private string _root = PathUtils.Root;
        private bool _disableEvents = true;
        /// <summary>
        ///   Database to use when loading data
        /// </summary>
        public string Database
        {
            get
            {
                return this._database;
            }
            set
            {
                this._database = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether to clean all local modifications on the update.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the <see cref="T:Sitecore.Data.Serialization.LoadOptions" /> forces the update; otherwise, <c>false</c>.
        /// </value>
        public bool ForceUpdate
        {
            get
            {
                return this._forceUpdate;
            }
            set
            {
                this._forceUpdate = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether the to use new IDs always when inserting new items.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if new IDs should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseNewID
        {
            get
            {
                return this._useNewId;
            }
            set
            {
                this._useNewId = value;
            }
        }
        /// <summary>
        ///   Custom serialization root for load operation.
        /// </summary>
        public string Root
        {
            get
            {
                return this._root;
            }
            set
            {
                this._root = value;
            }
        }
        /// <summary>
        /// Specifies whether database events should be suppressed when loading (which makes it much faster and is the default).
        /// </summary>
        public bool DisableEvents
        {
            get
            {
                return this._disableEvents;
            }
            set
            {
                this._disableEvents = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Data.Serialization.LoadOptions" /> class.
        /// </summary>
        public LoadOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Data.Serialization.LoadOptions" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public LoadOptions(LoadOptions options)
        {
            this._database = options.Database;
            this._root = options.Root;
            this._disableEvents = options.DisableEvents;
            this._forceUpdate = options.ForceUpdate;
            this._useNewId = options.UseNewID;
        }
        /// <summary>
        ///   Initializes a new instance of the <see cref="T:Sitecore.Data.Serialization.LoadOptions" /> class tied to a specific database and a custom serialization root.
        /// </summary>
        /// <param name="database">The database</param>
        /// <param name="root"></param>
        public LoadOptions(string root)
        {
            this._root = root;
        }
    }
}
