using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindtree.ItemWebApi.Pipelines.Configuration
{
    public static class Settings
    {
        public static class Security
        {
            public const string FieldRemoteReadAccessRight = "remote:fieldread";
            public const string IsEncryptedHttpHeader = "X-Scitemwebapi-Encrypted";
            public const string UserNameHttpHeader = "X-Scitemwebapi-Username";
            public const string UserPasswordHttpHeader = "X-Scitemwebapi-Password";
            public const string SaltKey = "ItemWebApi.SaltKey";
        }
        public static string ItemVersionHttpParameterName
        {
            get
            {
                return Sitecore.Configuration.Settings.GetSetting("ItemWebApi.ItemVersionHttpParameterName", "sc_itemversion");
            }
        }
        public static string LanguageHttpParameterName
        {
            get
            {
                return Sitecore.Configuration.Settings.GetSetting("ItemWebApi.LanguageHttpParameterName", "language");
            }
        }

        public static double CacheExpirationTime
        {
            get
            {
                string value = Sitecore.Configuration.Settings.GetSetting("ItemWebApi.CacheExpirationTime", "1800.00");
                return Convert.ToDouble(value);
            }
        }

        public static string sc_itemid
        {
            get
            {
                return "sc_itemid";
            }
        }

        public static string sc_database
        {
            get
            {
                return "sc_database";
            }
        }

        public static bool OutputExceptionDetails
        {
            get
            {
                return Sitecore.Configuration.Settings.GetBoolSetting("ItemWebApi.OutputExceptionDetails", false);
            }
        }

        public static string SaltKey
        {
            get
            {
                return Sitecore.Configuration.Settings.GetSetting(Security.SaltKey);
            }
        }

        public static string loadOptions
        {
            get
            {
                return "loadOptions";
            }
        }

        public static string enKey
        {
            get
            {
                return "enKey";
            }
        }

        public static string syncItem
        {
            get { return "syncItem"; }
        }

        public static string file
        {
            get { return "file"; }
        }

        public static string ContentTypeFormData
        {
            get
            {
                return "application/x-www-form-urlencoded";
            }
        }

        public static string ContentTypeMultipleFormData
        {
            get
            {
                return "multipart/form-data;";
            }
        }

        public static string RetainId
        {
            get
            {
                return "RetainID";
            }
        }
    }
}
