using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindtree.ItemWebApi.Pipelines
{
    internal class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message)
        {
            Assert.ArgumentNotNull(message, "message");
        }
    }
}
