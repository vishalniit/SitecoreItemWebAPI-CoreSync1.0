using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mindtree.ItemWebApi.Pipelines.Version.Create
{
    public class CreateVersionArgs : OperationArgs
    {

        public CreateVersionArgs(Item[] scope)
            : base(scope)
        {
            Assert.ArgumentNotNull(scope, "scope");
        }
    }
}
