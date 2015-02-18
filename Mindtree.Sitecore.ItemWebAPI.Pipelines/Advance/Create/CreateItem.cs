using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
namespace Mindtree.ItemWebApi.Pipelines.Advance.Create
{
    /// <summary>
    /// This class Create Item in Sitecore Instance using Create Args Parameter
    /// </summary>
    public class CreateItem : CreateProcessor
    {
        /// <summary>
        /// Function which process the CreateArgs and initiate the Private CreateNewItem Function
        /// </summary>
        /// <param name="arguments">Create Arguments</param>
        public override void Process(CreateArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            List<Item> list = new List<Item>(arguments.Scope.Length);
            Item[] scope = arguments.Scope;
            for (int i = 0; i < scope.Length; i++)
            {
                Item parent = scope[i];
                Item item = CreateItem.CreateNewItem(parent, arguments.Loadoptions, arguments.Syncitem, arguments.Context);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            arguments.Scope = list.ToArray();
        }

        private static Item CreateNewItem(Item parent, LoadOptions loadOptions, SyncItem syncItem, Sitecore.ItemWebApi.Context context)
        {
            Item itm = null;
            Assert.ArgumentNotNull(parent, "parent");
            Assert.ArgumentNotNull(loadOptions, "loadOptions");
            Assert.ArgumentNotNull(syncItem, "syncItem");
            try
            {
                itm = ItemSynchronization.PasteSyncItem(syncItem, loadOptions, true);
                if (itm != null)
                {
                    if (Common.Functions.IsMediaItem(context) && context.HttpContext != null && context.HttpContext.Request.Files != null)
                    {
                        MediaItem mediaItem = Common.Functions.UpdateMediaItem(itm, context.HttpContext);
                        if (mediaItem != null)
                        {
                            itm = mediaItem.InnerItem;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return itm;
        }
    }
}

