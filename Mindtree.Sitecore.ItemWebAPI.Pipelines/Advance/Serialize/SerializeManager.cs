using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Serialization.Exceptions;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.StringExtensions;
using System.Runtime.Serialization.Formatters.Binary;
using Sitecore.Data.Serialization.ObjectModel;
using System.Xml.Serialization;
using Mindtree.ItemWebApi.Pipelines.Advance.Serialize.Entities;
using Sitecore.Data.Events;
using Sitecore.Eventing;
namespace Mindtree.ItemWebApi.Pipelines.Advance.Serialize
{
    /// <summary>
    /// Class helps in DeSerialization of Objects and data
    /// received from the request.
    /// </summary>
    public static class SerializeManager
    {
        private static void DeserializationFinished(string databaseName)
        {
            EventManager.RaiseEvent<Sitecore.Data.Serialization.SerializationFinishedEvent>(new Sitecore.Data.Serialization.SerializationFinishedEvent());
            Sitecore.Data.Database database = Sitecore.Configuration.Factory.GetDatabase(databaseName, false);
            if (database != null)
            {
                database.RemoteEvents.Queue.QueueEvent<Sitecore.Data.Serialization.SerializationFinishedEvent>(new Sitecore.Data.Serialization.SerializationFinishedEvent());
            }
        }

        public static Sitecore.Data.Serialization.LoadOptions _loadOption { get; set; }

        /// <summary>
        /// Function which Desialize textual item into Sitecore.Data.Serialization.ObjectModel.SyncItem
        /// </summary>
        /// <param name="data">string:Textual representation of data</param>
        /// <param name="options">Custom.ItemWebAPI.Pipelines.Advance.Serialize.Entities:LoadOption which define how to Serialize</param>
        /// <returns>Sitecore.Data.Serialization.ObjectModel.SyncItem</returns>
        public static SyncItem DeSerializeItem(string data, LoadOptions options)
        {
            SyncItem result = null;
            if (data == null || data.Length == 0)
                return result;
            bool disabledLocally = Sitecore.Data.Serialization.ItemHandler.DisabledLocally;
            try
            {
                using (TextReader textReader = new StringReader(data))
                {
                    _loadOption = getLoadOption(options);
                    if (_loadOption.DisableEvents)
                    {
                        using (new EventDisabler())
                        {
                            Sitecore.Data.Serialization.ItemHandler.DisabledLocally = true;
                            result = ReadItem(new Sitecore.Data.Serialization.ObjectModel.Tokenizer(textReader), false);

                        }
                        //Sitecore.Data.Serialization.Manager.
                        DeserializationFinished(_loadOption.Database.Name);
                    }
                    else
                    {
                        Sitecore.Data.Serialization.ItemHandler.DisabledLocally = true;
                        result = SyncItem.ReadItem(new Sitecore.Data.Serialization.ObjectModel.Tokenizer(textReader));
                    }
                }
            }
            catch (ParentItemNotFoundException ex)
            {
                SerializeManager.LogLocalizedError("Cannot load item from path '{0}'. Possible reason: parent item with ID '{1}' not found.", new object[]
							{
								//PathUtils.UnmapItemPath(path, options.Root),
								ex.ParentID
							});
            }
            catch (ParentForMovedItemNotFoundException ex2)
            {
                SerializeManager.LogLocalizedError("Item from path '{0}' cannot be moved to appropriate location. Possible reason: parent item with ID '{1}' not found.", new object[]
							{
								//PathUtils.UnmapItemPath(path, options.Root),
								ex2.ParentID
							});
            }
            catch (Exception ex)
            {
                SerializeManager.LogLocalizedError(ex.Message, ex);
            }
            finally
            {
                Sitecore.Data.Serialization.ItemHandler.DisabledLocally = disabledLocally;
            }
            return result;
        }

        private static void LogLocalizedError(string message, params object[] parameters)
        {
            Assert.IsNotNullOrEmpty(message, "message");
            Job job = Context.Job;
            if (job != null)
            {
                job.Status.LogError(message.FormatWith(parameters));
                return;
            }
            Log.Error(message.FormatWith(parameters), new object());
        }

        /// <summary>
        /// Converts passed Bytes into Custom.ItemWebAPI.Pipelines.Advance.Serialize.Entities:LoadOption object
        /// Remember for this at origin it should be serialized using Binaryformatter
        /// </summary>
        /// <param name="arrBytes">Byte[]</param>
        /// <returns>Custom.ItemWebAPI.Pipelines.Advance.Serialize.Entities.LoadOptions</returns>
        public static LoadOptions DeSerializeLoadOptions(byte[] arrBytes)
        {
            LoadOptions loOptions = null;
            try
            {
                if (arrBytes != null && arrBytes.Count<byte>() > 0)
                {
                    MemoryStream memStream = new MemoryStream();
                    BinaryFormatter binForm = new BinaryFormatter();
                    memStream.Write(arrBytes, 0, arrBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    Object obj = (Object)binForm.Deserialize(memStream);
                    memStream.Close();
                    if (obj != null)
                        loOptions = obj as LoadOptions;
                }
            }
            catch (Exception)
            {
            }
            return loOptions;
        }

        /// <summary>
        /// Converts passed Serialized text into Custom.ItemWebAPI.Pipelines.Advance.Serialize.Entities:LoadOption object
        /// Remember for this at origin it should be serialized using XML Formatter
        /// </summary>
        /// <param name="arrBytes">XML text representation of LoadOption object</param>
        /// <returns>Custom.ItemWebAPI.Pipelines.Advance.Serialize.Entities.LoadOptions</returns>
        public static LoadOptions DeSerializeLoadOptions(string options)
        {
            LoadOptions loOptions = null;
            try
            {
                if (options != null && options.Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LoadOptions));
                    using (TextReader reader = new StringReader(options))
                    {
                        loOptions = (LoadOptions)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception)
            {
            }
            return loOptions;
        }

        private static Sitecore.Data.Serialization.LoadOptions getLoadOption(LoadOptions loOptions)
        {
            Sitecore.Data.Serialization.LoadOptions loadOptions = null;
            if (loOptions != null)
            {
                loadOptions = new Sitecore.Data.Serialization.LoadOptions();
                if (loOptions.Database != null && loOptions.Database.Length > 0)
                    loadOptions.Database = Common.Functions.GetDatabase(loOptions.Database);
                loadOptions.DisableEvents = loOptions.DisableEvents;
                loadOptions.ForceUpdate = loOptions.ForceUpdate;
                loadOptions.Root = loOptions.Root;
                loadOptions.UseNewID = loOptions.UseNewID;
            }
            return loadOptions;
        }

        private static string BeginReadHeaders(Tokenizer reader)
        {
            string text = reader.NextLine();
            while (text.StartsWith("----", StringComparison.InvariantCulture))
            {
                text = reader.NextLine();
            }
            return text;
        }

        private static void ReadHeaders(Tokenizer reader, string s, Action<string, string> updateValue)
        {
            while (!string.IsNullOrEmpty(s))
            {
                string[] array = s.Split(new char[]
		{
			':'
		}, 2);
                if (array.Length == 2)
                {
                    updateValue(array[0], array[1]);
                }
                s = reader.NextLine();
            }
        }

        private static Dictionary<string, string> ReadHeaders(Tokenizer reader)
        {
            string s = BeginReadHeaders(reader);
            Dictionary<string, string> result = new Dictionary<string, string>();
            ReadHeaders(reader, s, delegate(string key, string value)
            {
                result[key] = value.TrimStart(new char[0]);
            });
            return result;
        }

        private static SyncItem ReadItem(Tokenizer reader, bool assertVersion)
        {
            SyncItem syncItem = null;
            while (reader.Line != null && reader.Line.Length == 0)
            {
                reader.NextLine();
            }
            if (reader.Line == null || reader.Line != "----item----")
            {
                throw new Exception("Format error: serialized stream does not start with ----item----");
            }
            syncItem = new SyncItem();
            Dictionary<string, string> dictionary = ReadHeaders(reader);
            syncItem.ID = dictionary["id"];
            int num;
            if (assertVersion && !string.IsNullOrEmpty(dictionary["version"]) && int.TryParse(dictionary["version"], out num) && SyncItem.FormatVersion < num)
            {
                throw new Exception(string.Concat(new object[]
		{
			"The file was generated using a newer version of Serialization. (",
			SyncItem.FormatVersion,
			" < ",
			num,
			")"
		}));
            }
            syncItem.ItemPath = dictionary["path"];
            SyncItem result;
            try
            {
                syncItem.DatabaseName = dictionary["database"];
                syncItem.ParentID = dictionary["parent"];
                syncItem.Name = dictionary["name"];
                syncItem.BranchId = dictionary["master"];
                syncItem.TemplateID = dictionary["template"];
                syncItem.TemplateName = dictionary["templatekey"];
                reader.NextLine();
                while (reader.Line == "----field----")
                {
                    SyncField syncField = SyncField.ReadField(reader);
                    if (syncField != null)
                    {
                        syncItem.SharedFields.Add(syncField);
                    }
                }
                while (reader.Line == "----version----")
                {
                    SyncVersion syncVersion = ReadVersion(reader);
                    if (syncVersion != null)
                    {
                        syncItem.Versions.Add(syncVersion);
                    }
                }
                result = syncItem;
            }
            catch (Exception innerException)
            {
                throw new Exception("Error reading item: " + syncItem.ItemPath, innerException);
            }
            return result;
        }

        private static SyncVersion ReadVersion(Tokenizer reader)
        {
            SyncVersion syncVersion = new SyncVersion();
            SyncVersion result;
            try
            {
                Dictionary<string, string> dictionary = ReadHeaders(reader);
                syncVersion.Language = dictionary["language"];
                syncVersion.Version = dictionary["version"];
                syncVersion.Revision = dictionary["revision"];
                reader.NextLine();
                while (reader.Line == "----field----")
                {
                    SyncField syncField = SyncField.ReadField(reader);
                    if (syncField != null)
                    {
                        syncVersion.Fields.Add(syncField);
                    }
                }
                result = syncVersion;
            }
            catch (Exception innerException)
            {
                throw new Exception(string.Format("Failed to load version {0} for language {1}", syncVersion.Version, syncVersion.Language), innerException);
            }
            return result;
        }
    }
}
