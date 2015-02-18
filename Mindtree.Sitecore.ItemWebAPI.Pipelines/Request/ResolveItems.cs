using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.ItemWebApi;
using Sitecore.ItemWebApi.Pipelines.Request;
using Sitecore.Web;
using System;
namespace Mindtree.ItemWebApi.Pipelines.Request
{
    public class ResolveItems : RequestProcessor
    {
        public override void Process(RequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            arguments.Items = ResolveItems.GetItems(arguments);
        }
        private static Item[] GetItems(RequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            Database database = arguments.Context.Database;
            if (database == null)
                database = Common.Functions.GetDatabase();
            Item item = Common.Functions.GetItem(arguments);
            string queryString = WebUtil.GetQueryString("sc_database");
            if (!string.IsNullOrEmpty(queryString) && !string.Equals(queryString, database.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new AccessDeniedException(string.Format("Access to the '{0}' database is denied. Only members of the Sitecore Client Users role can switch databases.", queryString));
            }
            string text = WebUtil.GetQueryString("query", null);
            if (text != null)
            {
                Item[] result;
                if (text.StartsWith("fast:"))
                {
                    text = text.Substring(5);
                    try
                    {

                        result = (database.SelectItems(text) ?? new Item[0]);
                        return result;
                    }
                    catch
                    {
                        throw new BadRequestException(string.Format("Bad Sitecore fast query: ({0}).", text));
                    }
                }
                if (item == null)
                {
                    Logger.Warn("Context item not resolved.");
                    return new Item[0];
                }
                try
                {
                    result = (Query.SelectItems(text) ?? new Item[0]);
                }
                catch
                {
                    throw new BadRequestException(string.Format("Bad Sitecore query ({0}).", text));
                }
                return result;
            }
            if (item != null)
            {
                return new Item[]
				{
					item
				};
            }
            Logger.Warn("Context item not resolved.");
            return new Item[0];
        }
    }
}
