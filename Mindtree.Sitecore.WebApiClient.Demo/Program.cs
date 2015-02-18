using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.IO;
using Mindtree.Sitecore.WebApi.Client.Data;
using Mindtree.Sitecore.WebApi.Client.Entities;
using Mindtree.Sitecore.WebApi.Client.Interfaces;
using Mindtree.Sitecore.WebApi.Client.Net;
using Mindtree.Sitecore.WebApi.Client.Security;
using Mindtree.Sitecore.WebApi.Client;
using Mindtree.Sitecore.WebApi.Client.Serialization;
using Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities;
namespace Sitecore.SharedSource.WebApiClient.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //const string host = "http://audit.brand.com/";
            const string host = "http://test.brand.com/";
            var context = new SitecoreDataContext(host);

            var secureContext = new AuthenticatedSitecoreDataContext(
                                            host,
                                            new SitecoreCredentials
                                            {
                                                UserName = "sitecore\\admin",
                                                Password = "b"
                                            });
            //CreateItemSample(secureContext);
            //UpdateItemIdSample(secureContext);
            //// expression query example

            //ExpressionQuerySample(secureContext);

            //// single item query example

            //ItemQuerySample(context);

            //// working with fields example

            //FieldsSample(context);

            //// secure context



            //// credentials

            //CredentialsSample(secureContext);

            //// creating

            //CreateItemSample(secureContext);

            //// updating using item ids

            UpdateItemIdSample(secureContext);

            //// updating using queries

            //UpdateItemExpressionSample(secureContext);

            //// deleting

            //DeleteQuerySample(secureContext);

            //// encrypted credentials

            //var encryptedSecureContext = new AuthenticatedSitecoreDataContext(
            //    host,
            //    new SitecoreCredentials
            //        {
            //            UserName = "siteore\\admin",
            //            Password = "b",
            //            EncryptHeaders = true
            //        });

            //EncryptedCredentialsSample(encryptedSecureContext);
        }

        private static void EncryptedCredentialsSample(AuthenticatedSitecoreDataContext context)
        {
            var query = new SitecoreItemQuery(SitecoreQueryType.Read)
            {
                ItemId = "{11111111-1111-1111-1111-111111111111}",
                QueryScope = new[] { SitecoreItemScope.Self }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                WebApiItem item = response.Result.Items[0];

                Wl("path", item.Path);
                Nl();
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        private static void CredentialsSample(AuthenticatedSitecoreDataContext context)
        {
            var query = new SitecoreItemQuery(SitecoreQueryType.Read)
            {
                ItemId = "{11111111-1111-1111-1111-111111111111}",
                QueryScope = new[] { SitecoreItemScope.Self }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                WebApiItem item = response.Result.Items[0];

                Wl("path", item.Path);
                Nl();
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        private static void FieldsSample(SitecoreDataContext context)
        {
            var query = new SitecoreItemQuery(SitecoreQueryType.Read)
            {
                ItemId = "{11111111-1111-1111-1111-111111111111}",
                QueryScope = new[] { SitecoreItemScope.Self }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                WebApiItem item = response.Result.Items[0];

                Wl("path", item.Path);
                Nl();

                WriteFields(item);
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        private static void ItemQuerySample(SitecoreDataContext context)
        {
            var query = new SitecoreItemQuery(SitecoreQueryType.Read)
            {
                ItemId = "{11111111-1111-1111-1111-111111111111}",
                QueryScope = new[] { SitecoreItemScope.Self, SitecoreItemScope.Children }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                foreach (WebApiItem item in response.Result.Items)
                {
                    Wl("path", item.Path);
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        /// <summary>
        /// Item creation sample
        /// </summary>
        /// <para>
        ///     Requires an authenticated data context
        /// </para>
        /// <para>
        ///     The user must have create permissions on the parent
        /// </para>
        /// <param name="context">The context.</param>
        private static void CreateItemSample(AuthenticatedSitecoreDataContext context)
        {
            //Dictionary<string, string> fieldstoUpdate = new Dictionary<string, string>();
            //fieldstoUpdate.Add("Sitemap Item Order", "test Value");
            //var query = new SitecoreCreateQuery
            //{
            //    ItemId = "{CBC4876C-EBDD-4472-899B-DC09933E2ED7}",
            //    Template = "{790F9670-EE94-40C4-8233-4168528341B7}",
            //    Database = "master",
            //    Name="test",
            //    FieldsToUpdate = fieldstoUpdate,
            //    FieldsToReturn = new List<string>
            //                                        {
            //                                            "Name",
            //                                            ""
            //                                        }
            //};

            //Dictionary<string, string> fieldstoUpdate = new Dictionary<string, string>();
            //fieldstoUpdate.Add("Sitemap Item Order", "test Value");
            //fieldstoUpdate.Add("Sitemap Title", "test Value");
            //var query = new SitecoreCreateQuery
            //{                
            //    ItemId = "{CBC4876C-EBDD-4472-899B-DC09933E2ED7}",                
            //    Database = "master",
            //    FieldsToUpdate = fieldstoUpdate,
            //    Language = "en",
            //    FieldsToReturn = new List<string>
            //                                        {
            //                                            "Name",
            //                                            "Sitemap Title"
            //                                        }
            //};

            Dictionary<string, string> fieldstoUpdate = new Dictionary<string, string>();
            fieldstoUpdate.Add("Sitemap Item Order", "test Value");
            fieldstoUpdate.Add("Sitemap Title", "test Value");
            LoadOptions loadOptions = new LoadOptions();
            loadOptions.Database = "master";
            string loadoption=SerializeManager.SerializeLoadOption(loadOptions);
            string dataSyncItem= string.Empty;
            var query = new SitecoreAdvanceCreateQuery(Mindtree.Sitecore.WebApi.Client.SitecoreQueryType.AdvanceCreate, Mindtree.Sitecore.WebApi.Client.ResponseFormat.Json)
            {
                ItemId = "{0DE95AE4-41AB-4D01-9EB0-67441B7C2450}",
                Database = "master",
                loadOptions = loadoption,
                syncItem = dataSyncItem,
                RetainID = true,
            };

            //Media Version Create Query
            //FileStream fs = new FileStream(@"C:\data\unnamed.gif", FileMode.OpenOrCreate);
            //Dictionary<string, string> fieldstoUpdate = new Dictionary<string, string>();
            //fieldstoUpdate.Add("Width", "114");
            //fieldstoUpdate.Add("Height", "114");
            //fieldstoUpdate.Add("Alt", "Test File");
            //var query = new SitecoreCreateVersionQuery
            //{
            //    Items = "{4989E299-AE7B-42D5-A030-DAB9B0FF564F}",
            //    ItemId = "{4989E299-AE7B-42D5-A030-DAB9B0FF564F}",
            //    Database = "master",
            //    FieldsToUpdate = fieldstoUpdate,
            //    MediaItemStream=fs,
            //    Language = "en",
            //    FieldsToReturn = new List<string>
            //                                        {
            //                                            "Size",
            //                                            "Extention"
            //                                        }
            //};

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                foreach (WebApiItem item in response.Result.Items)
                {
                    Wl("path", item.Path);
                    WriteFields(item);
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        /// <summary>
        /// Item updating sample using item id
        /// </summary>
        /// <para>
        ///     Requires an authenticated data context
        /// </para>
        /// <para>
        ///     The user must have write permissions on the item
        /// </para>
        /// <param name="context">The context.</param>
        private static void UpdateItemIdSample(AuthenticatedSitecoreDataContext context)
        {
            //var query = new SitecoreItemQuery(SitecoreQueryType.Update)
            //{
            //    ItemId = "{11111111-1111-1111-1111-111111111111}",
            //    QueryScope = new[]
            //                     {
            //                         SitecoreItemScope.Self,
            //                         SitecoreItemScope.Children
            //                     },
            //    Database = "master",
            //    FieldsToUpdate = new Dictionary<string, string>
            //                                       {
            //                                           { "Field Name", "Value" },
            //                                           { "{11111111-1111-1111-1111-111111111111}", "Value" }
            //                                       },
            //    FieldsToReturn = new List<string> 
            //                                        {
            //                                            "Field Name",
            //                                            "{11111111-1111-1111-1111-111111111111}"
            //                                        }
            //};

            Dictionary<string, string> fieldstoUpdate = new Dictionary<string, string>();
            fieldstoUpdate.Add("Sitemap Item Order", "test Value-1");
            var query = new SitecoreItemQuery(SitecoreQueryType.Update)
            {
                ItemId = "{CBC4876C-EBDD-4472-899B-DC09933E2ED7}",
                Database = "master",
                Language = "en-US",
                ItemVersion = 4,
                FieldsToUpdate = fieldstoUpdate,
                FieldsToReturn = new List<string>
                                                    {
                                                        "Name",
                                                        ""
                                                    }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                foreach (WebApiItem item in response.Result.Items)
                {
                    Wl("path", item.Path);
                    WriteFields(item);
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        /// <summary>
        /// Item updating sample using Sitecore query
        /// </summary>
        /// <para>
        ///     Requires an authenticated data context
        /// </para>
        /// <para>
        ///     The user must have write permissions on the item
        /// </para>
        /// <param name="context">The context.</param>
        private static void UpdateItemExpressionSample(AuthenticatedSitecoreDataContext context)
        {
            var query = new SitecoreExpressionQuery(SitecoreQueryType.Update)
            {
                Query = "/sitecore/content/Home",
                QueryScope = new[]
                                 {
                                     SitecoreItemScope.Self
                                 },
                Database = "master",
                FieldsToUpdate = new Dictionary<string, string>
                                                   {
                                                       { "Field Name", "Value" },
                                                       { "{11111111-1111-1111-1111-111111111111}", "Value" }
                                                   },
                FieldsToReturn = new List<string> 
                                                    {
                                                        "Field Name",
                                                        "{11111111-1111-1111-1111-111111111111}"
                                                    }
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                foreach (WebApiItem item in response.Result.Items)
                {
                    Wl("path", item.Path);
                    WriteFields(item);
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        /// <summary>
        /// Item deletion sample
        /// </summary>
        /// <para>
        ///     Requires an authenticated data context
        /// </para>
        /// <para>
        ///     The user must have delete permissions on the item
        /// </para>
        /// <param name="context">The context.</param>
        private static void DeleteQuerySample(AuthenticatedSitecoreDataContext context)
        {
            // WARNING: all items in the query scope and their descendants will be deleted
            // only items in the query scope count toward the response count
            var query = new SitecoreItemQuery(SitecoreQueryType.Delete)
            {
                ItemId = "{11111111-1111-1111-1111-111111111111}",
                QueryScope = new[] { SitecoreItemScope.Self },
                Database = "web"
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                Wl("deletion count", response.Result.Count);

                if (response.Result.ItemIds != null)
                {
                    foreach (string id in response.Result.ItemIds)
                    {
                        Wl("id", id);
                    }
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        private static void ExpressionQuerySample(SitecoreDataContext context)
        {
            var query = new SitecoreExpressionQuery(SitecoreQueryType.Read)
            {
                Query = "/sitecore/content/Gillette",
                Payload = SitecorePayload.Content,
                QueryScope = new[] { SitecoreItemScope.Self },
                Database = "master",
                Language = "en-US",
                ItemVersion = 1,
                ExtractBlob = true
            };

            ISitecoreWebResponse response = context.GetResponse<SitecoreWebResponse>(query);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                WriteResponseMeta(response);

                foreach (WebApiItem item in response.Result.Items)
                {
                    Wl("path", item.Path);
                }
            }
            else
            {
                WriteError(response);
            }

            Nl();
        }

        #region console helper methods

        /// <summary>
        /// Writes the error message from a failed response to the console.
        /// </summary>
        /// <param name="response">The response.</param>
        private static void WriteError(ISitecoreWebResponse response)
        {
            Wl("status", (int)response.StatusCode);
            if (response.Error != null)
                Wl("message", response.Error.Message);
        }

        /// <summary>
        /// Writes the fields of the passed item to the console.
        /// </summary>
        /// <param name="item">The item.</param>
        private static void WriteFields(WebApiItem item)
        {
            foreach (KeyValuePair<string, WebApiField> field in item.Fields)
            {
                Wl("fieldname", field.Value.Name);
                Wl("fieldvalue", field.Value.Value);
                Nl();
            }
        }

        /// <summary>
        /// Writes the response meta to the console.
        /// </summary>
        /// <param name="response">The response.</param>
        private static void WriteResponseMeta(ISitecoreWebResponse response)
        {
            Wl("uri", response.Info.Uri.PathAndQuery);
            Wl("status", (int)response.StatusCode);
            Wl("description", response.StatusDescription);
            Wl("time", response.Info.ResponseTime.ToString());
            Wl("count", response.Result.TotalCount);
            Nl();
            Wl("Results");
            Nl();
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        private static void Wl(string label, string value)
        {
            Console.WriteLine("{0}: {1}", label, value);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        private static void Wl(string label, int value)
        {
            Wl(label, value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="value">The value.</param>
        private static void Wl(string value)
        {
            Console.WriteLine(value + ":");
        }

        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        private static void Nl()
        {
            Console.WriteLine(Environment.NewLine);
        }

        #endregion
    }
}

