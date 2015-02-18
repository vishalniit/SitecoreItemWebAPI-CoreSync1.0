using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindtree.ItemWebApi.Pipelines.Configuration;
using Sitecore.SecurityModel;
using Sitecore.ItemWebApi.Pipelines.Request;
using System.Web;
using System.IO;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.ItemWebApi;
namespace Mindtree.ItemWebApi.Pipelines.Common
{
    /// <summary>
    /// Common Function used in service and other CoreSync Funtionalities
    /// Note few of the function depend upon the context
    /// and you need to handle occuring exception by yourself.
    /// </summary>
    public static class Functions
    {
        public static string GetCoreSyncSetting(string fieldName)
        {
            string value = "";
            if (fieldName != null && fieldName.Length > 0)
            {
                Item itm = GetCoreSyncSettingItem();
                if (itm != null)
                {
                    value = itm.Fields[fieldName].Value;
                }
            }
            return value;
        }

        public static Item GetCoreSyncSettingItem()
        {
            Item value = null;
            string settingpath = Sitecore.Configuration.Settings.GetSetting("CoreSync.WebSettingItem", "/sitecore/system/Modules/WebsiteSync");
            string dbname = Sitecore.Configuration.Settings.GetSetting("CoreSync.WebSettingItemDB", "master");
            value = GetItemPath(settingpath, dbname);
            return value;
        }

        /// <summary>
        /// This function identify if the context item is Media Item
        /// </summary>
        /// <param name="context"></param>
        /// <returns>bool is media item or not</returns>
        public static bool IsMediaItem(Sitecore.ItemWebApi.Context context)
        {
            return IsMediaItem(context.Item);
        }

        public static bool IsMediaItem(Item item)
        {
            return (Sitecore.Resources.Media.MediaManager.HasMediaContent(item) || item.Paths.IsMediaItem);
        }

        /// <summary>
        /// This function get item version number indicated in the querystring parameter 'sc_itemversion'
        /// </summary>
        /// <returns>Sitecore.Data.Version</returns>
        public static Sitecore.Data.Version GetItemVersion()
        {
            string queryString = WebUtil.GetQueryString(Settings.ItemVersionHttpParameterName, null);
            int versionNumber = 0;
            if (queryString != null && queryString.Length > 0)
                versionNumber = System.Convert.ToInt32(queryString);
            return GetItemVersion(versionNumber);
        }

        /// <summary>
        /// This function returns item version number specified as passed number parameter
        /// </summary>
        /// <param name="dbname">int, Version Number</param>
        /// <returns>Sitecore.Data.Database</returns>
        public static Sitecore.Data.Version GetItemVersion(int versionNum)
        {
            if (versionNum == 0)
            {
                return Sitecore.Data.Version.Latest;
            }
            Sitecore.Data.Version version;
            bool condition = Sitecore.Data.Version.TryParse(versionNum.ToString(), out version);
            Assert.IsTrue(condition, "Cannot recognize item version.");
            Assert.IsTrue(version.Number > 0, "Item version is wrong.");
            return version;
        }

        /// <summary>
        /// This function get database indicated in the querystring parameter 'sc_database'
        /// </summary>
        /// <returns>Sitecore.Data.Database</returns>
        public static Database GetDatabase()
        {
            Database result = null;
            result = Sitecore.Context.Database;
            if (result == null)
                result = GetDatabase(WebUtil.GetQueryString(Settings.sc_database));
            return result;
        }

        /// <summary>
        /// This function returns the specific database based on passed name parameter
        /// </summary>
        /// <param name="dbname">string, Database name</param>
        /// <returns>Sitecore.Data.Database</returns>
        public static Database GetDatabase(string dbname)
        {
            Database result = null;
            string dbcachekey = dbname + "-dbkey";
            if (dbname == null)
                return result;
            if (!APICache.isCacheExist(dbcachekey, Common.APICache.RegionName))
            {
                result = Sitecore.Configuration.Factory.GetDatabase(dbname);
                APICache.AddToAPICache(dbcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
            }
            else
            {
                result = APICache.GetAPICachedItem<Database>(dbcachekey, Common.APICache.RegionName);
            }
            return result;
        }

        /// <summary>
        /// This function get language indicated in the querystring parameter 'language'
        /// </summary>        
        /// <returns>Sitecore.Globalization.Language</returns>
        public static Language GetLanguage()
        {
            Language result = null;
            result = Sitecore.Context.Language;
            if (result == null)
                result = GetLanguage(WebUtil.GetQueryString(Settings.LanguageHttpParameterName));
            Assert.ArgumentNotNull(result, "items");
            return result;
        }

        /// <summary>
        /// This function returns the specific language based on passed name parameter
        /// </summary>
        /// <param name="dbname">string, Language name</param>
        /// <returns>Sitecore.Globalization.Language</returns>
        public static Language GetLanguage(string language)
        {
            Language result = null;
            string langcachekey = language + "-langkey";
            if (language == null)
                return result;
            if (!APICache.isCacheExist(langcachekey, Common.APICache.RegionName))
            {
                result = LanguageManager.GetLanguage(language);
                APICache.AddToAPICache(langcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
            }
            else
            {
                result = APICache.GetAPICachedItem<Language>(langcachekey, Common.APICache.RegionName);
            }
            return result;
        }

        /// <summary>
        /// Returns an item always from the context or from queryString 'sc_itemid'
        /// </summary>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItem()
        {
            Item result = null;
            result = Sitecore.Context.Item;
            if (result != null)
                result = result.Versions.GetLatestVersion();
            if (result == null || !WebUtil.GetQueryString(Settings.sc_itemid).Equals(result.ID.Guid.ToString(), StringComparison.InvariantCultureIgnoreCase))
                result = GetItem(WebUtil.GetQueryString(Settings.sc_itemid));
            return result;
        }

        /// <summary>
        /// Returns an item from the Request Args based on following precedence
        /// If not found in context and item found in context is not equal to the query string parameter of 'sc_itemid'
        /// then get that item using query string and get always latest version of it.
        /// </summary>
        /// <param name="ra">RequestArgs</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItem(RequestArgs ra)
        {
            Item result = null;
            if (ra.Context != null)
                result = ra.Context.Item;
            if (result == null || !WebUtil.GetQueryString(Settings.sc_itemid).Equals(result.ID.Guid.ToString(), StringComparison.InvariantCultureIgnoreCase))
                result = GetItem(WebUtil.GetQueryString(Settings.sc_itemid));
            if (result != null)
                result = result.Versions.GetLatestVersion();
            return result;
        }

        /// <summary>
        /// Returns an item always from the provided ItemID  
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="ItemID">Item id in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItem(string ItemID)
        {
            Item result = null;
            using (new SecurityDisabler())
            {
                string itemcachekey = string.Empty;
                Database db = GetDatabase();
                Language lang = GetLanguage();
                Sitecore.Data.Version version = GetItemVersion();
                if (db != null && lang != null && version != null)
                {
                    itemcachekey = ItemID + lang.Name + version.Number.ToString();
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(ItemID, lang, version);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
                else if (db != null && lang != null)
                {
                    itemcachekey = ItemID + lang.Name + "0";
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(ItemID, lang);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
                else if (db != null)
                {
                    itemcachekey = ItemID + "default" + "0";
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(ItemID);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    result = db.GetItem(ItemID);
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an item always from the provided ItemID  
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="path">Item path in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItemPath(string path)
        {
            Item result = null;
            using (new SecurityDisabler())
            {
                string itemcachekey = string.Empty;
                Database db = GetDatabase();
                Language lang = GetLanguage();
                Sitecore.Data.Version version = GetItemVersion();
                if (db != null && lang != null && version != null)
                {
                    itemcachekey = path.Trim() + lang.Name + version.Number.ToString();
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(path, lang, version);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
                else if (db != null && lang != null)
                {
                    itemcachekey = path.Trim() + lang.Name + "0";
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(path, lang);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
                else if (db != null)
                {
                    itemcachekey = path.Trim() + "default" + "0";
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(path);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    result = db.GetItem(path);
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an item always from the provided ItemID  
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="ItemID">Item id in string</param>
        /// <param name="dbName">dbName in string</param>
        /// <param name="language">language in string</param>
        /// <param name="versionNum">versionNum in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItemPath(string path, string dbName = "master", string language = "en", int versionNum = 1)
        {
            Item result = null;
            using (new SecurityDisabler())
            {
                string itemcachekey = string.Empty;
                Database db = GetDatabase(dbName);
                Language lang = GetLanguage(language);
                Sitecore.Data.Version version = GetItemVersion(versionNum);
                if (db != null && lang != null && version != null)
                {
                    itemcachekey = path.Trim() + lang.Name + version.Number.ToString();
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(path, lang, version);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an item always from the provided ItemID  
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="ItemID">Item id in string</param>
        /// <param name="dbName">dbName in string</param>
        /// <param name="language">language in string</param>
        /// <param name="versionNum">versionNum in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item GetItem(string ItemID, string dbName = "master", string language = "en", int versionNum = 1)
        {
            Item result = null;
            using (new SecurityDisabler())
            {
                string itemcachekey = string.Empty;
                Database db = GetDatabase(dbName);
                Language lang = GetLanguage(language);
                Sitecore.Data.Version version = GetItemVersion(versionNum);
                if (db != null && lang != null && version != null)
                {
                    itemcachekey = ItemID + lang.Name + version.Number.ToString();
                    if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                    {
                        result = db.GetItem(ItemID, lang, version);
                        APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                    }
                    else
                    {
                        result = APICache.GetAPICachedItem<Item>(itemcachekey, Common.APICache.RegionName);
                    }
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns an item aray from the provided ItemID collection 
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="ItemID">Item id collection in string</param>
        /// <param name="dbName">dbName in string</param>
        /// <param name="language">language in string</param>
        /// <param name="versionNum">versionNum in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item[] GetItems(string[] ItemID, string dbName = "master", string language = "en", int versionNum = 1)
        {
            List<Item> liItems = new List<Item>();
            if (ItemID != null && ItemID.Count<string>() > 0)
            {
                foreach (var item in ItemID)
                {
                    liItems.Add(GetItem(item, dbName, language, versionNum));
                }
            }
            return liItems.ToArray<Item>();
        }

        /// <summary>
        /// Returns an item aray from the provided ItemID collection 
        /// This will get the db from the context or look from query string
        /// This will get the language from the context or look from query string
        /// This will get the version from the query string or take the default version which is latest
        /// </summary>
        /// <param name="ItemID">Item id collection in string</param>
        /// <param name="dbName">dbName in string</param>
        /// <param name="language">language in string</param>
        /// <param name="versionNum">versionNum in string</param>
        /// <returns>Sitecore.Data.Items.Item</returns>
        public static Item[] GetItems(string[] ItemIDs)
        {
            List<Item> liItems = new List<Item>();
            foreach (var item in ItemIDs)
            {
                liItems.Add(GetItem(item));
            }
            return liItems.ToArray<Item>();
        }

        /// <summary>
        /// This function let knows service if it need to retain passed GUID during Item Creation
        /// </summary>
        /// <returns>bool either to retain ID or not</returns>
        public static bool IsRetainID()
        {
            bool result = false;
            if (WebUtil.GetQueryString(Settings.RetainId, result.ToString()).Equals("true", StringComparison.InvariantCultureIgnoreCase))
                result = true;
            return result;
        }

        /// <summary>
        /// Get Sitecore DataTemplate based on passed TemplateID
        /// </summary>
        /// <param name="TemplateID">string, TemplateID in string GUID format</param>
        /// <returns>TemplateItem</returns>
        public static TemplateItem GetTemplate(string TemplateID)
        {
            TemplateItem result = null;
            result = GetItem(TemplateID);
            if (result == null)
            {
                using (new SecurityDisabler())
                {
                    string itemcachekey = string.Empty;
                    try
                    {
                        itemcachekey = TemplateID + "-templkey";
                        if (!APICache.isCacheExist(itemcachekey, Common.APICache.RegionName))
                        {
                            result = GetDatabase().GetTemplate(TemplateID);
                            APICache.AddToAPICache(itemcachekey, result, APICachePriority.Default, Common.APICache.RegionName);
                        }
                        else
                        {
                            result = APICache.GetAPICachedItem<TemplateItem>(itemcachekey, Common.APICache.RegionName);
                        }
                    }
                    catch (Exception)
                    {
                        result = null;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// This function update the passed media item from the file received in the context
        /// </summary>
        /// <param name="_itm">Siecore.Data.Items.Item, Media Item</param>
        /// <param name="httpContext">System.Web.HttpContext, Current context from request</param>
        /// <returns>MediaItem, Updated Media Item</returns>
        public static MediaItem UpdateMediaItem(Item _itm, HttpContext httpContext)
        {
            MediaItem mediaItem = null;
            if (_itm != null && httpContext != null)
            {
                try
                {
                    mediaItem = new MediaItem(_itm);
                    mediaItem.BeginEdit();
                    if (httpContext.Request.Form["alt"] != null)
                        mediaItem.Alt = httpContext.Request.Form["alt"].ToString();
                    if (httpContext.Request.Form["Width"] != null)
                        mediaItem.InnerItem["Width"] = httpContext.Request.Form["Width"].ToString();
                    if (httpContext.Request.Form["Height"] != null)
                        mediaItem.InnerItem["Height"] = httpContext.Request.Form["Height"].ToString();
                    HttpFileCollection files = httpContext.Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFile httpPostedFile = files[i];
                        if (httpPostedFile.ContentLength != 0)
                        {
                            string fileName = httpPostedFile.FileName;
                            Stream inputStream = httpPostedFile.InputStream;
                            string extension = FileUtil.GetExtension(fileName);
                            try
                            {
                                mediaItem.Size = inputStream.Length;
                                Media media = MediaManager.GetMedia(mediaItem);
                                media.SetStream(inputStream, extension);
                                break;
                            }
                            catch
                            {
                                Logger.Warn("Cannot create the versioned media item.");
                            }
                        }
                    }
                    mediaItem.EndEdit();
                }
                catch { }
            }
            return mediaItem;
        }

        private static Item getHistoryItem(Sitecore.Data.Engines.HistoryEntry item, string dbname, string language = "en", string path = "")
        {
            //If History entry item is null then return null
            if (item == null || dbname == null)
                return null;
            if (language != null && language.Length > 0)
            {
                //if language doesn't match then return null
                if (!item.ItemLanguage.Name.Equals(language, StringComparison.CurrentCultureIgnoreCase))
                    return null;
            }
            if (path != null && path.Length > 0)
            {
                string compPath = item.ItemPath + @"/";
                //if path doesn't fall under given path which means location is not correct then return null
                if (!compPath.Contains(path))
                    return null;
            }
            return GetItem(item.ItemId.Guid.ToString(), dbname, language);
        }

        private static Item getHistoryItem(Sitecore.Data.Engines.HistoryEntry item, string dbname, int version, string language = "en", string path = "")
        {
            //If History entry item is null then return null
            if (item == null || dbname == null)
                return null;
            if (language != null && language.Length > 0)
            {
                //if language doesn't match then return null
                if (!item.ItemLanguage.Name.Equals(language, StringComparison.CurrentCultureIgnoreCase))
                    return null;
            }
            //if version doesn't match then return null
            if (item.ItemVersion.Number != version)
                return null;

            if (path != null && path.Length > 0)
            {
                string compPath = item.ItemPath + @"/";
                //if path doesn't fall under given path which means location is not correct then return null
                if (!compPath.Contains(path))
                    return null;
            }
            return GetItem(item.ItemId.Guid.ToString(), dbname, language, version);
        }

        public static Sitecore.Collections.HistoryEntryCollection getUniqueLatestVersionFromHistoryCollection(Sitecore.Collections.HistoryEntryCollection data)
        {
            Sitecore.Collections.HistoryEntryCollection hec = null;
            if (data != null && data.Count > 0)
            {
                hec = new Sitecore.Collections.HistoryEntryCollection();
                //This will find out unique items in history collection
                var unique = data.DistinctBy(x => x.ItemId);
                foreach (var item in unique)
                {
                    //This will found those items with same ID in the original data collection
                    List<Sitecore.Data.Engines.HistoryEntry> sameItems = (from he in data
                                                                          where he.ItemId == item.ItemId
                                                                          select he).ToList<Sitecore.Data.Engines.HistoryEntry>();
                    //This will find out the latest version possible in the sameItems collection
                    Sitecore.Data.Engines.HistoryEntry historyEntry = sameItems.Aggregate((i1, i2) => i1.ItemVersion.Number > i2.ItemVersion.Number ? i1 : i2);
                    //Finally add the value to real history collection
                    if (historyEntry != null)
                        hec.Add(historyEntry);
                }
            }
            return hec;
        }

        public static Item[] getHistoryAtPoint(string dbname, DateTime atPoint, Sitecore.Data.Engines.HistoryAction ha, string language = "en", string path = "")
        {
            DateTime from = DateTime.MinValue;
            List<Item> li = new List<Item>();
            Sitecore.Collections.HistoryEntryCollection hec = getAllHistoryItems(dbname, from, atPoint, ha);
            hec = getUniqueLatestVersionFromHistoryCollection(hec);
            if (hec != null && hec.Count > 0)
            {
                foreach (var item in hec)
                {
                    Item itm = getHistoryItem(item, dbname, item.ItemVersion.Number, language, path);
                    if (itm != null)
                        li.Add(itm);
                }
            }
            return li.ToArray<Item>();
        }

        public static Sitecore.Collections.HistoryEntryCollection getAllHistoryItems(string dbname, DateTime from, DateTime to, Sitecore.Data.Engines.HistoryAction ha)
        {
            Sitecore.Collections.HistoryEntryCollection hec = new Sitecore.Collections.HistoryEntryCollection();
            var entries = HistoryManager.GetHistory(GetDatabase(dbname), from, to);
            foreach (var item in entries)
            {
                if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item && ha == item.Action)
                {
                    hec.Add(item);
                }
            }
            return hec;
        }

        public static Item[] getAllHistoryItems(string dbname, DateTime from, DateTime to, Sitecore.Data.Engines.HistoryAction ha = Sitecore.Data.Engines.HistoryAction.AddedVersion, string language = "en", string path = "")
        {
            List<Item> li = new List<Item>();
            var entries = getAllHistoryItems(dbname, from, to, ha);
            foreach (var item in entries)
            {
                Item itm = getHistoryItem(item, dbname, item.ItemVersion.Number, language, path);
                if (itm != null)
                    li.Add(itm);
            }
            return li.ToArray<Item>();
        }

        /// <summary>
        /// Get updated Items only from History Table based on from Date to Now Date
        /// </summary>
        /// <param name="dbname">string, Sitecore Database name, example master</param>
        /// <param name="from">DateTime, in UTC format to retrive the items since</param>
        /// <param name="language">string, retrieve specific language item only</param>
        /// <param name="version">int, Item Version Number, Passed item version will only be effective if exact match of path given with history entry found otherwise latest version will be asked for all other items</param>
        /// <param name="path">string, retrive those items only which are under certain location</param>
        /// <returns>Item array</returns>
        public static Item[] getUpdatedHistoryItems(string dbname, DateTime from, string language = "en", string path = "")
        {
            List<Item> li = new List<Item>();
            var now = DateTime.UtcNow;
            Sitecore.Collections.HistoryEntryCollection entries = HistoryManager.GetHistory(GetDatabase(dbname), from, now);
            entries = getUniqueLatestVersionFromHistoryCollection(entries);
            if (entries != null && entries.Count > 0)
            {
                foreach (var item in entries)
                {
                    if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item)
                    {
                        if (item.Action == Sitecore.Data.Engines.HistoryAction.AddedVersion || item.Action == Sitecore.Data.Engines.HistoryAction.AddedVersion || item.Action == Sitecore.Data.Engines.HistoryAction.Saved)
                        {
                            Item itm = getHistoryItem(item, dbname, item.ItemVersion.Number, language, path);
                            if (itm != null)
                            {
                                li.Add(itm);
                            }
                        }
                    }
                }
            }
            if (li != null && li.Count > 0)
            {
                li = li.DistinctBy(x => x.ID).ToList<Item>();
            }
            return li.ToArray<Item>();
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        /// <summary>
        /// Get Created Items only from History Table based on from Date to Now Date
        /// </summary>
        /// <param name="dbname">string, Sitecore Database name, example master</param>
        /// <param name="from">DateTime, in UTC format to retrive the items since</param>
        /// <param name="language">string, retrieve specific language item only</param>
        /// <param name="version">int, Item Version Number</param>
        /// <param name="path">string, retrive those items only which are under certain location</param>
        /// <returns>Item array</returns>
        public static Item[] getCreatedHistoryItems(string dbname, DateTime from, string language = "en", int version = 1, string path = "")
        {
            List<Item> li = new List<Item>();
            var now = DateTime.UtcNow;
            var entries = HistoryManager.GetHistory(GetDatabase(dbname), from, now);
            foreach (var item in entries)
            {
                if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item)
                {
                    if (item.Action == Sitecore.Data.Engines.HistoryAction.Created)
                    {
                        Item itm = getHistoryItem(item, dbname, version, language, path);
                        if (itm != null)
                        {
                            li.Add(itm);
                        }
                    }
                }
            }
            return li.ToArray<Item>();
        }

        /// <summary>
        /// Get Deleted Items only from History Table based on from Date to Now Date
        /// </summary>
        /// <param name="dbname">string, Sitecore Database name, example master</param>
        /// <param name="from">DateTime, in UTC format to retrive the items since</param>
        /// <param name="language">string, retrieve specific language item only</param>
        /// <param name="version">int, Item Version Number</param>
        /// <param name="path">string, retrive those items only which are under certain location</param>
        /// <returns>Item array</returns>
        public static Item[] getDeletedHistoryItems(string dbname, DateTime from, string language = "en", int version = 1, string path = "")
        {
            List<Item> li = new List<Item>();
            var now = DateTime.UtcNow;
            var entries = HistoryManager.GetHistory(GetDatabase(dbname), from, now);
            foreach (var item in entries)
            {
                if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item)
                {
                    if (item.Action == Sitecore.Data.Engines.HistoryAction.Deleted)
                    {
                        Item itm = getHistoryItem(item, dbname, version, language, path);
                        if (itm != null)
                        {
                            li.Add(itm);
                        }
                    }
                }
            }
            return li.ToArray<Item>();
        }

        /// <summary>
        /// Get Copied Items only from History Table based on from Date to Now Date
        /// </summary>
        /// <param name="dbname">string, Sitecore Database name, example master</param>
        /// <param name="from">DateTime, in UTC format to retrive the items since</param>
        /// <param name="language">string, retrieve specific language item only</param>
        /// <param name="version">int, Item Version Number</param>
        /// <param name="path">string, retrive those items only which are under certain location</param>
        /// <returns>Item array</returns>
        public static Item[] getCopiedHistoryItems(string dbname, DateTime from, string language = "en", int version = 1, string path = "")
        {
            List<Item> li = new List<Item>();
            var now = DateTime.UtcNow;
            var entries = HistoryManager.GetHistory(GetDatabase(dbname), from, now);
            foreach (var item in entries)
            {
                if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item)
                {
                    if (item.Action == Sitecore.Data.Engines.HistoryAction.Copied)
                    {
                        Item itm = getHistoryItem(item, dbname, version, language, path);
                        if (itm != null)
                        {
                            li.Add(itm);
                        }
                    }
                }
            }
            return li.ToArray<Item>();
        }

        /// <summary>
        /// Get Moved Items only from History Table based on from Date to Now Date
        /// </summary>
        /// <param name="dbname">string, Sitecore Database name, example master</param>
        /// <param name="from">DateTime, in UTC format to retrive the items since</param>
        /// <param name="language">string, retrieve specific language item only</param>
        /// <param name="version">int, Item Version Number</param>
        /// <param name="path">string, retrive those items only which are under certain location</param>
        /// <returns>Item array</returns>
        public static Item[] getMovedHistoryItems(string dbname, DateTime from, string language = "en", int version = 1, string path = "")
        {
            List<Item> li = new List<Item>();
            var now = DateTime.UtcNow;
            var entries = HistoryManager.GetHistory(GetDatabase(dbname), from, now);
            foreach (var item in entries)
            {
                if (item.Category == Sitecore.Data.Engines.HistoryCategory.Item)
                {
                    if (item.Action == Sitecore.Data.Engines.HistoryAction.Moved)
                    {
                        Item itm = getHistoryItem(item, dbname, version, language, path);
                        if (itm != null)
                        {
                            li.Add(itm);
                        }
                    }
                }
            }
            return li.ToArray<Item>();
        }

    }
}
