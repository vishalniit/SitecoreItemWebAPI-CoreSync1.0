using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.ItemWebApi;
using Sitecore.ItemWebApi.Configuration;
using Sitecore.ItemWebApi.Pipelines.Request;
using Sitecore.ItemWebApi.Serialization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Web;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace Mindtree.ItemWebApi.Pipelines.HttpRequest
{
    public class LaunchRequest : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            try
            {
                Sitecore.ItemWebApi.Context current = Sitecore.ItemWebApi.Context.Current;
                if (current != null)
                {
                    current.HttpContext = arguments.Context;
                    current.Database = Common.Functions.GetDatabase();
                    current.Item = Common.Functions.GetItem();
                    current.Language = Common.Functions.GetLanguage();
                    CorePipeline.Run("itemWebApiRequest", new RequestArgs());
                    arguments.AbortPipeline();
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception exception)
            {
                ErrorReporter.SendErrorMessage(exception);
            }
        }

        //private Item GetContextItem(HttpRequestArgs arguments)
        //{
        //    Item item = Sitecore.Context.Item;
        //    if (item == null || item.ID.Guid.ToString().Equals("e0bb067e-19c1-4e9f-a9ed-d9a476e1a521", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        if (arguments.Context.Request.QueryString["sc_itemid"] == null || arguments.Context.Request.QueryString["sc_database"] == null || arguments.Context.Request.QueryString["language"] == null)
        //        {
        //            return null;
        //        }
        //        string itemid = arguments.Context.Request.QueryString["sc_itemid"].ToString();
        //        string _database = arguments.Context.Request.QueryString["sc_database"].ToString();
        //        string _lang = arguments.Context.Request.QueryString["language"].ToString();
        //        if (itemid == null || _database == null || _lang == null)
        //            return null;
        //        Database db = Sitecore.Configuration.Factory.GetDatabase(_database);
        //        Language lang = LanguageManager.GetLanguage(_lang);
        //        if (db == null)
        //            return null;
        //        Sitecore.Data.Version version = this.GetItemVersion();
        //        if (!item.Versions.GetVersionNumbers().Contains(version))
        //        {
        //            version = Sitecore.Data.Version.Latest;
        //        }
        //        item = db.GetItem(item.ID, lang, version);
        //    }
        //    else
        //    {
        //        Sitecore.Data.Version version = this.GetItemVersion();
        //        Database db = item.Database;
        //        if (!item.Versions.GetVersionNumbers().Contains(version))
        //        {
        //            version = Sitecore.Data.Version.Latest;
        //        }
        //        item = db.GetItem(item.ID, item.Language, version);
        //    }
        //    return item;
        //}
    }

    //public class CustomRewriteUrl : Sitecore.Pipelines.PreprocessRequest.PreprocessRequestProcessor
    //{
    //    public override void Process(Sitecore.Pipelines.PreprocessRequest.PreprocessRequestArgs arguments)
    //    {
    //        Assert.ArgumentNotNull(arguments, "arguments");
    //        try
    //        {
    //            string localPath = arguments.Context.Request.Url.LocalPath;
    //            if (localPath.StartsWith("/-/item/"))
    //            {
    //                Context current = new Context
    //                {
    //                    Serializer = new JsonSerializer(),
    //                    Version = CustomRewriteUrl.GetVersion(localPath)
    //                };
    //                Context.Current = current;
    //                CustomRewriteUrl.Rewrite(arguments.Context);
    //            }
    //        }
    //        catch (Exception exception)
    //        {
    //            Logger.Error(exception);
    //        }
    //    }
    //    private static int GetVersion(string path)
    //    {
    //        Assert.ArgumentNotNull(path, "path");
    //        string text = path.TrimStart(new char[]
    //        {
    //            '/'
    //        }).Split(new char[]
    //        {
    //            '/'
    //        })[2];
    //        Assert.IsTrue(text.StartsWith("v"), "Version token is wrong.");
    //        string s = text.Replace("v", string.Empty);
    //        int result;
    //        bool condition = int.TryParse(s, out result);
    //        Assert.IsTrue(condition, "Version not recognized.");
    //        return result;
    //    }
    //    private static void Rewrite(HttpContext context)
    //    {
    //        Assert.ArgumentNotNull(context, "context");
    //        Uri url = context.Request.Url;
    //        string text = url.LocalPath.TrimStart(new char[]
    //        {
    //            '/'
    //        });
    //        string[] array = text.Split(new char[]
    //        {
    //            '/'
    //        });
    //        int num = array.Length - 3;
    //        string[] array2 = new string[num];
    //        Array.Copy(array, 3, array2, 0, num);
    //        string path = string.Format("/{0}", string.Join("/", array2));
    //        string query = url.Query.TrimStart(new char[]
    //        {
    //            '?'
    //        });
    //        Sitecore.Text.UrlString urlString = new Sitecore.Text.UrlString
    //        {
    //            Path = path,
    //            Query = query
    //        };
    //        WebUtil.RewriteUrl(urlString.ToString());
    //    }
    //}

    //internal class JsonSerializer : ISerializer
    //{
    //    public string SerializedDataMediaType
    //    {
    //        get
    //        {
    //            return "application/json";
    //        }
    //    }
    //    public string Serialize(object value)
    //    {
    //        Assert.ArgumentNotNull(value, "value");
    //        return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(value);
    //    }
    //}
}
