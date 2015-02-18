using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Create;
using Sitecore;
using Sitecore.ItemWebApi.Pipelines;
using Sitecore.Exceptions;
using Sitecore.Data.Managers;

namespace Mindtree.ItemWebApi.Pipelines.Version.Create
{ 
    public class Create : CreateVersionProcessor
    {
        public override void Process(CreateVersionArgs arguments)
        {
            Assert.ArgumentNotNull(arguments, "arguments");
            List<Item> list = new List<Item>(arguments.Scope.Length);
            Item[] scope = arguments.Scope;
            for (int i = 0; i < scope.Length; i++)
            {
                Item itm = scope[i];
                if (itm != null)
                {
                    Item _itm=itm.Versions.AddVersion();
                    if (_itm != null)
                        list.Add(_itm);
                }
            }
            arguments.Scope = list.ToArray();
        }       
    }
}
