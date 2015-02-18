using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.ItemWebApi.Pipelines;
using System;
namespace Mindtree.ItemWebApi.Pipelines.Advance.Create
{
    /// <summary>
    /// Create Arguments for Advance Create Operation Pipeline
    /// </summary>
	public class CreateArgs : OperationArgs
	{
        private LoadOptions loadOptions;
		/// <summary>
		/// You can get the Load Options once CreateArgs called.
		/// </summary>
        public LoadOptions Loadoptions
		{
			get
			{
                return Assert.ResultNotNull<LoadOptions>(this.loadOptions);
			}
			set
			{
				Assert.IsNotNull(value, "loadOptions");
                this.loadOptions = value;
			}
		}

        private SyncItem syncItem;
        /// <summary>
        /// You can get the SyncItem once CreateArgs called.
        /// </summary>
        public SyncItem Syncitem
		{
			get
			{
                return Assert.ResultNotNull<SyncItem>(this.syncItem);
			}
			set
			{
				Assert.IsNotNull(value, "SyncItem");				
                this.syncItem = value;
			}
		}

        /// <summary>
        /// Constructor Method which initialise the Create Args Properties
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="loadoptions"></param>
        /// <param name="Item"></param>
		public CreateArgs(Item[] scope, LoadOptions loadoptions, SyncItem Item) : base(scope)
		{
			Assert.ArgumentNotNull(scope, "scope");
            Assert.ArgumentNotNull(loadoptions, "loadoptions");
            Assert.ArgumentNotNull(Item, "Item");
            this.Syncitem = Item;
            this.Loadoptions = loadoptions;
		}
	}
}

