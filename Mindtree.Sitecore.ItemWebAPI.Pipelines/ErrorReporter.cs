using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.ItemWebApi;
using Sitecore.ItemWebApi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Mindtree.ItemWebApi.Pipelines.Extentions;

namespace Mindtree.ItemWebApi.Pipelines
{
    internal static class ErrorReporter
    {
        public static void SendErrorMessage(Exception exception)
        {
            Assert.ArgumentNotNull(exception, "exception");
            Logger.Error(exception);
            Context current = Context.Current;
            Assert.IsNotNull(current, "Item Web API context is null.");
            Dynamic dynamic = new Dynamic();
            dynamic["message"] = exception.Message;
            //overwrite the code from sitecore dissaseembly - need to based on settings in the config
            if (true)
            {
                dynamic["type"] = exception.GetType().FullName;
                dynamic["stackTrace"] = exception.StackTrace;
            }
            Dynamic dynamic2 = new Dynamic();
            dynamic2["statusCode"] = (int)ErrorReporter.GetStatusCode(exception);
            dynamic2["error"] = dynamic;
            ISerializer serializer = current.Serializer;
            string s = serializer.Serialize(dynamic2);
            HttpResponse response = HttpContext.Current.Response;
            response.ContentType = serializer.SerializedDataMediaType;
            response.StatusCode = 200;
            response.Clear();
            response.DisableCaching();
            response.Write(s);
            response.End();
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            Assert.ArgumentNotNull(exception, "exception");
            if (exception is BadRequestException)
            {
                return HttpStatusCode.BadRequest;
            }
            bool flag = exception is AccessDeniedException || exception is SecurityException || exception is UnauthorizedAccessException;
            if (!flag)
            {
                return HttpStatusCode.InternalServerError;
            }
            if (!Sitecore.Context.User.IsAuthenticated)
            {
                return HttpStatusCode.Unauthorized;
            }
            return HttpStatusCode.Forbidden;
        }
    }
}
