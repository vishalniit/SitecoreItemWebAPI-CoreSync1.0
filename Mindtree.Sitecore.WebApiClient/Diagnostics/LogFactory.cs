using System;
using System.IO;
using System.Text;

namespace Mindtree.Sitecore.WebApi.Client.Diagnostics
{
    /// <summary>
    /// Factory class for logging messages
    /// </summary>
    public static class Log
    {
        private static StringBuilder _info;
        public static StringBuilder Info
        {
            get
            {
                if (_info == null)
                    _info = new StringBuilder();
                return _info;

            }
            set
            {
                _info = value;
            }
        }

        private static StringBuilder _warn;
        public static StringBuilder Warn
        {
            get
            {
                if (_warn == null)
                    _warn = new StringBuilder();
                return _warn;

            }
            set
            {
                _warn = value;
            }
        }

        private static StringBuilder _error;
        public static StringBuilder Error
        {
            get
            {
                if (_error == null)
                    _error = new StringBuilder();
                return _error;

            }
            set
            {
                _error = value;
            }
        }

        /// <summary>
        /// Logs the specified message as information.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteInfo(string message)
        {
            Info.Append(message);            
        }

        /// <summary>
        /// Logs the specified message as a warning.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteWarn(string message)
        {
            Warn.Append(message);
        }

        /// <summary>
        /// Logs the specified message as an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        public static void WriteError(string message, Exception ex)
        {
            Error.Append(message);
            Error.AppendLine();
            Error.Append(ex.StackTrace);
            Error.AppendLine();
        }
        
    }
}
