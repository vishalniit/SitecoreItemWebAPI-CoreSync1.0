using Mindtree.ItemWebApi.Pipelines.Configuration;
using Mindtree.ItemWebApi.Pipelines.Encrypt;
using Mindtree.ItemWebApi.Pipelines.Version.Create;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.IO;
using Sitecore.ItemWebApi;
using Sitecore.ItemWebApi.Pipelines.Create;
using Sitecore.ItemWebApi.Pipelines.Read;
using Sitecore.ItemWebApi.Pipelines.Request;
using Sitecore.ItemWebApi.Pipelines.Update;
using Sitecore.ItemWebApi.Security;
using Sitecore.Pipelines;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mindtree.ItemWebApi.Pipelines.Request
{
    public class CustomResolveAction : Sitecore.ItemWebApi.Pipelines.Request.ResolveAction
    {
        public override void Process(RequestArgs requestArgs)
        {
            Assert.ArgumentNotNull(requestArgs, "requestArgs");
            string method = GetMethod(requestArgs.Context);
            if (requestArgs.Context.Settings.Access == AccessType.ReadOnly && method != "get")
            {
                throw new AccessDeniedException("The operation is not allowed.");
            }
            string a;
            if ((a = method) != null)
            {
                if (a == "delete")
                {
                    this.ExecuteDeleteRequest(requestArgs);
                    return;
                }
                if (a == "get")
                {
                    this.ExecuteReadRequest(requestArgs);
                    return;
                }
                if (a == "post")
                {
                    if (Common.Functions.IsRetainID())
                        this.ExecuteCreateRequest(requestArgs);
                    else
                        base.ExecuteCreateRequest(requestArgs);
                    return;
                }
                if (a == "ver")
                {
                    this.ExecuteCreateVersionRequest(requestArgs);
                    return;
                }
                if (!(a == "put"))
                {
                    return;
                }
                this.ExecuteUpdateRequest(requestArgs);
            }
        }

        protected virtual void ExecuteCreateVersionRequest(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (Common.Functions.IsMediaItem(args.Context))
            {
                this.CreateMediaVersionItems(args);
                return;
            }
            else
            {
                this.CreateVersionItems(args);
                return;
            }
            throw new BadRequestException("The specified Content-Type is not supported.");
        }

        private void CreateVersionItems(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item[] scope = args.Scope;
            Assert.IsNotNull(scope, "The scope is null.");
            string queryString = WebUtil.GetQueryString("items");
            Item[] items = null;
            if (queryString != null && queryString.Length > 0 && queryString.Contains('|'))
                items = Common.Functions.GetItems(queryString.Split('|'));
            else if (queryString != null && queryString.Length > 0)
                items = new Item[1]; items[0] = Common.Functions.GetItem(queryString);
            if (items == null)
            {
                throw new ArgumentException("Items not found.");
            }
            Item[] scope2 = (scope.Length > 0) ? new Item[]
			{
				scope.First<Item>()
			} : new Item[0];
            CreateVersionArgs createVersionArgs = new CreateVersionArgs(scope2)
            {
                Scope = items
            };
            CorePipeline.Run("itemWebApiCreateVersion", createVersionArgs);
            args.Result = createVersionArgs.Result;
        }

        private void CreateMediaVersionItems(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = args.Context.Item;
            if (item == null)
            {
                throw new BadRequestException("The specified location not found.");
            }
            Database database = args.Context.Database;
            Assert.IsNotNull(database, "Database not resolved.");
            Sitecore.Globalization.Language currentLanguage = args.Context.Language;
            if (!item.Access.CanCreate())
            {
                throw new AccessDeniedException(string.Format("Access denied (access right: 'item:create', item: '{0}')", item.Paths.ParentPath));
            }
            HttpFileCollection files = args.Context.HttpContext.Request.Files;
            List<Item> list = new List<Item>();
            Item _itm = item.Versions.AddVersion();
            MediaItem mediaItem = Common.Functions.UpdateMediaItem(_itm, args.Context.HttpContext);
            if (mediaItem != null)
            {
                list.Add(mediaItem.InnerItem);
                ReadArgs readArgs = new ReadArgs(list.ToArray());
                CorePipeline.Run("itemWebApiRead", readArgs);
                args.Result = readArgs.Result;
            }
        }

        private string GetMethod(Context context)
        {
            Assert.ArgumentNotNull(context, "context");
            return context.HttpContext.Request.HttpMethod.ToLower();
        }

        private void CreateAdvanceItem(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item[] scope = args.Scope;
            Assert.IsNotNull(scope, "The scope is null.");

            Item[] scope2 = (scope.Length > 0) ? new Item[]
			{
				scope.First<Item>()
			} : new Item[0];
            if (scope2 != null && scope2.Length > 0)
            {
                Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities.LoadOptions loadOptions = null;
                SyncItem syncItem = null;
                try
                {
                    if (args.Context.HttpContext != null && args.Context.HttpContext.Request != null && args.Context.HttpContext.Request.Form != null)
                    {
                        string _enkey = string.Empty;
                        if (args.Context.HttpContext.Request.Form[Settings.enKey] != null)
                        {
                            _enkey = args.Context.HttpContext.Request.Form[Settings.enKey];
                        }

                        if (args.Context.HttpContext.Request.Form[Settings.loadOptions] != null)
                        {
                            string data = args.Context.HttpContext.Request.Form[Settings.loadOptions];
                            loadOptions = Mindtree.ItemWebApi.Pipelines.Advance.Serialize.SerializeManager.DeSerializeLoadOptions(StringCipher.Decrypt(data, _enkey));
                        }
                        if (args.Context.HttpContext.Request.Form[Settings.syncItem] != null)
                        {
                            string dataSyncItem = args.Context.HttpContext.Request.Form[Settings.syncItem];
                            syncItem = Mindtree.ItemWebApi.Pipelines.Advance.Serialize.SerializeManager.DeSerializeItem(StringCipher.Decrypt(dataSyncItem, _enkey), loadOptions);
                        }
                    }
                }
                catch (Exception)
                { }
                //args.Context.HttpContext.Request.Params[""]

                Mindtree.ItemWebApi.Pipelines.Advance.Create.CreateArgs createArgs = new Mindtree.ItemWebApi.Pipelines.Advance.Create.CreateArgs(scope2, Mindtree.ItemWebApi.Pipelines.Advance.Serialize.SerializeManager._loadOption, syncItem)
                {
                    Scope = scope2
                };
                CorePipeline.Run("itemWebApiCreateAdvance", createArgs);
                args.Result = createArgs.Result;
            }
            else
                throw new BadRequestException("Parent Item doesn't Exist");
            
        }

        private void UpdateMediaItem(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item item = args.Context.Item;
            if (item == null)
            {
                throw new BadRequestException("The specified location not found.");
            }
            Database database = args.Context.Database;
            Assert.IsNotNull(database, "Database not resolved.");
            Sitecore.Globalization.Language currentLanguage = args.Context.Language;
            if (!item.Access.CanCreate())
            {
                throw new AccessDeniedException(string.Format("Access denied (access right: 'item:create', item: '{0}')", item.Paths.ParentPath));
            }
            HttpFileCollection files = args.Context.HttpContext.Request.Files;
            List<Item> list = new List<Item>();
            MediaItem mediaItem = Common.Functions.UpdateMediaItem(item, args.Context.HttpContext);
            if (mediaItem != null)
            {
                list.Add(mediaItem.InnerItem);
                ReadArgs readArgs = new ReadArgs(list.ToArray());
                CorePipeline.Run("itemWebApiRead", readArgs);
                args.Result = readArgs.Result;
            }
        }

        protected override void ExecuteCreateRequest(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            this.CreateAdvanceItem(args);
            return;
        }

        protected override void ExecuteUpdateRequest(RequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (Common.Functions.IsMediaItem(args.Context))
            {
                UpdateMediaItem(args);
                return;
            }
            else
            {
                UpdateArgs updateArgs = new UpdateArgs(args.Scope);
                CorePipeline.Run("itemWebApiUpdate", updateArgs);
                args.Result = updateArgs.Result;
                return;
            }
            throw new BadRequestException("The specified Content-Type is not supported.");
        }
    }
}
