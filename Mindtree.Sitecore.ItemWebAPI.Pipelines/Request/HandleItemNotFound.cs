using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Request;
using Sitecore.Web;
using System;
namespace Mindtree.ItemWebApi.Pipelines.Request
{
    public class HandleItemNotFound : RequestProcessor
    {
        public override void Process(RequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            string queryString = WebUtil.GetQueryString("sc_itemid", null);
            if (queryString == null)
            {
                string text = arguments.Context.HttpContext.Request.QueryString[null];
                if (text != null)
                {
                    if (Array.Exists<string>(text.Split(new char[]
					{
						','
					}), (string s) => s.ToLower() == "sc_itemid"))
                    {
                        arguments.Context.Item = null;
                    }
                }
                return;
            }
            ID itemId;
            if (!ID.TryParse(queryString, out itemId))
            {
                arguments.Context.Item = null;
                return;
            }
            if (Sitecore.Context.Database.GetItem(itemId) == null)
            {                
                arguments.Context.Item = null;
            }
        }
    }
}