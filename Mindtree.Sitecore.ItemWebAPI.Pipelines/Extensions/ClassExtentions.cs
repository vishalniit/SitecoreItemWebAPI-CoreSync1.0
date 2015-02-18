using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Mindtree.ItemWebApi.Pipelines.Extentions
{
    internal static class ClassExtensions
    {
        public static void DisableCaching(this HttpResponse response)
        {
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
        }
    }
}
