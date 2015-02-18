using Mindtree.Sitecore.WebApi.Client.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization.Exceptions;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities;
using Sitecore.Data.Managers;
using Sitecore.Data.Fields;
using Sitecore;
using Sitecore.Data.Serialization.ObjectModel;
using Sitecore.Data.Serialization;
namespace Mindtree.Sitecore.WebApi.Client.Serialization
{
    /// <summary>
    /// Serialization Helper class which helps serialize Sitecore.Data.Items.Item
    /// object into bytes, string.
    /// It also helps to serialize LoadOptions class into string.
    /// </summary>
    public class SerializeManager
    {
        /// <summary>
        /// This function Serialize Sitecore.Data.Items.Item into byte array.
        /// Though here string is been originated from the TextWriter wrapped in Stream class.
        /// </summary>
        /// <param name="itm">byte[]</param>
        /// <returns>string representation of byte[] </returns>
        public static byte[] SerializeItem(Item itm)
        {
            byte[] data = null;
            if (itm == null)
                return data;
            using (new SecurityDisabler())
            {
                Stream ss = null; TextWriter tw = null;
                try
                {
                    ss = new MemoryStream();
                    tw = new System.IO.StreamWriter(ss);
                    try
                    {
                        ItemSynchronization.WriteItem(itm, tw);
                    }
                    catch (System.Exception ex)
                    {
                        Log.WriteError(ex.Message, ex);
                    }
                    data = new byte[ss.Length];
                    ss.Read(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message, ex);
                }
                finally
                {
                    try
                    {
                        if (tw != null)
                            tw.Dispose();
                        if (ss != null)
                        {
                            ss.Close(); ss.Dispose();
                        }
                    }
                    catch { }
                }
            }
            return data;
        }

        public static void WriteHeader(string header, object value, TextWriter writer)
        {
            writer.WriteLine("{0}: {1}", header, value);
        }

        public static string GetSplitter(string key)
        {
            return string.Format("----{0}----", key);
        }

        public static void WriteSplitter(string key, TextWriter writer)
        {
            writer.WriteLine(GetSplitter(key));
        }

        public static void WriteNewLine(TextWriter writer)
        {
            writer.WriteLine();
        }

        public static void WriteText(string text, TextWriter writer)
        {
            writer.WriteLine(text);
        }

        public static void SerializeField(TextWriter writer, SyncField syncField)
        {
            
            WriteSplitter("field", writer);
            WriteHeader("field", syncField.FieldID, writer);
            WriteHeader("name", syncField.FieldName, writer);
            WriteHeader("key", syncField.FieldKey, writer);
            WriteHeader("content-length", syncField.FieldValue.Length, writer);
            WriteNewLine(writer);
            WriteText(syncField.FieldValue, writer);
        }

        public static void Serialize(TextWriter writer, SyncItem syncItem)
        {
            WriteSplitter("item", writer);
            WriteHeader("version", SyncItem.FormatVersion, writer);
            WriteHeader("id", syncItem.ID, writer);
            WriteHeader("database", syncItem.DatabaseName, writer);
            WriteHeader("path", syncItem.ItemPath, writer);
            WriteHeader("parent", syncItem.ParentID, writer);
            WriteHeader("name", syncItem.Name, writer);
            WriteHeader("master", syncItem.BranchId, writer);
            WriteHeader("template", syncItem.TemplateID, writer);
            WriteHeader("templatekey", syncItem.TemplateName, writer);
            WriteNewLine(writer);
            foreach (SyncField current in syncItem.SharedFields)
            {
                SerializeField(writer, current);
            }
            foreach (SyncVersion current2 in syncItem.Versions)
            {
                SerializeVersion(writer, current2);
            }
        }

        public static void SerializeVersion(TextWriter writer, SyncVersion syncVersion)
        {
            WriteSplitter("version", writer);
            WriteHeader("language", syncVersion.Language, writer);
            WriteHeader("version", syncVersion.Version, writer);
            WriteHeader("revision", syncVersion.Revision, writer);
            writer.WriteLine();
            foreach (SyncField current in syncVersion.Fields)
            {
                SerializeField(writer, current);
            }
        }

        /// <summary>
        /// This function Serialize Sitecore.Data.Items.Item into string.
        /// Though here string is been originated from the TextWriter class.
        /// </summary>
        /// <param name="itm">Sitecore.Data.Items.Item</param>
        /// <returns>string representation of Sitecore.Data.Items.Item </returns>
        public static string SerializeIteminString(Item itm)
        {
            string data = null;
            if (itm == null)
                return data;
            using (new SecurityDisabler())
            {
                Stream ss = null; TextWriter tw = null;
                try
                {
                    ss = new MemoryStream();
                    tw = new System.IO.StreamWriter(ss);
                    try
                    {
                        //Here I am using Sitecore Default Item Synchronization though i have method created in this class using which you
                        //you can do yourself also.
                        ItemSynchronization.WriteItem(itm, tw);
                        //Flushing the Memory is essential otherwise you are risking reading empty stream or partial data loss
                        tw.Flush();
                        ss.Flush();
                    }
                    catch (System.Exception ex)
                    {
                        Log.WriteError(ex.Message, ex);
                    }
                    //ss.Position = 0;
                    ss.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(ss))
                    {                        
                        data = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message, ex);
                }
                finally
                {
                    try
                    {
                        if (tw != null)
                            tw.Dispose();
                        if (ss != null)
                        {
                            ss.Close(); ss.Dispose();
                        }
                    }
                    catch { }
                }
            }
            return data;
        }

        public static SyncItem BuildSyncItem(Item item)
        {
            SyncItem syncItem = new SyncItem();
            syncItem.ID = item.ID.ToString();
            syncItem.DatabaseName = item.Database.Name;
            syncItem.ParentID = item.ParentID.ToString();
            syncItem.Name = item.Name;
            syncItem.BranchId = item.BranchId.ToString();
            syncItem.TemplateID = item.TemplateID.ToString();
            syncItem.TemplateName = item.TemplateName;
            syncItem.ItemPath = item.Paths.Path;
            item.Fields.ReadAll();
            item.Fields.Sort();
            foreach (Field field in item.Fields)
            {
                if (TemplateManager.GetTemplate(item).GetField(field.ID) != null && field.Shared)
                {
                    string fieldValue = GetFieldValue(field);
                    if (fieldValue != null)
                    {
                        syncItem.AddSharedField(field.ID.ToString(), field.Name, field.Key, fieldValue, true);
                    }
                }
            }
            Item[] versions = item.Versions.GetVersions(true);
            Array.Sort<Item>(versions, new Comparison<Item>(CompareVersions));
            Item[] array = versions;
            for (int i = 0; i < array.Length; i++)
            {
                Item version = array[i];
                BuildVersion(syncItem, version);
            }
            return syncItem;
        }

        private static int CompareVersions(Item left, Item right)
        {
            int num = left.Language.Name.CompareTo(right.Language.Name);
            if (num == 0)
            {
                num = left.Version.Number.CompareTo(right.Version.Number);
            }
            return num;
        }

        /// <summary>
        /// Builds the version.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="version">The version.</param>
        private static void BuildVersion(SyncItem item, Item version)
        {
            SyncVersion syncVersion = item.AddVersion(version.Language.ToString(), version.Version.ToString(), version.Statistics.Revision);
            if (syncVersion == null)
            {
                return;
            }
            version.Fields.ReadAll();
            version.Fields.Sort();
            foreach (Field field in version.Fields)
            {
                if (TemplateManager.GetTemplate(version).GetField(field.ID) != null && !field.Shared)
                {
                    string fieldValue = GetFieldValue(field);
                    if (fieldValue != null)
                    {
                        syncVersion.AddField(field.ID.ToString(), field.Name, field.Key, fieldValue, true);
                    }
                }
            }
        }

        private static string GetFieldValue(Field field)
        {
            string value = field.GetValue(false, false);
            if (value == null)
            {
                return null;
            }
            if (!field.IsBlobField)
            {
                return value;
            }
            Stream blobStream = field.GetBlobStream();
            if (blobStream == null)
            {
                return null;
            }
            string result;
            using (blobStream)
            {
                byte[] array = new byte[blobStream.Length];
                blobStream.Read(array, 0, array.Length);
                result = System.Convert.ToBase64String(array, Base64FormattingOptions.InsertLineBreaks);
            }
            return result;
        }

        /// <summary>
        /// This helps in Serialize the LoadOption object 
        /// into string, though string is XML representation of object.
        /// Here XML Serializer is used.
        /// </summary>
        /// <param name="loOptions">Custom.ItemWebApi.Pipelines.Advance.Serialize.Entities.LoadOptions Object</param>
        /// <returns>XML based string representation of LoadOption object</returns>
        public static string SerializeLoadOption(Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities.LoadOptions loOptions)
        {
            string data = string.Empty;
            if (loOptions != null)
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities.LoadOptions));
                StringWriter sww = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sww);
                xsSubmit.Serialize(writer, loOptions);
                data = sww.ToString();
            }
            return data;
        }

        /// <summary>
        /// This Function Helps serialize any serializable object using Binary Formatter
        /// </summary>
        /// <param name="obj">System.Object</param>
        /// <returns>Byte Array representation of object</returns>
        public static byte[] SerializeObject(Object obj)
        {
            byte[] data = null;
            IFormatter formatter = new BinaryFormatter();
            Stream ms = new MemoryStream();
            formatter.Serialize(ms, obj);
            data = new byte[ms.Length];
            ms.Read(data, 0, data.Length);
            ms.Close();
            return data;
        }
    }

}
