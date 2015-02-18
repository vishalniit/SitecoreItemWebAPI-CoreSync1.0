using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Read;
using Sitecore.Pipelines;
using System;
namespace Mindtree.ItemWebApi.Pipelines.Advance.Create
{
    /// <summary>
    /// This class set the result item in Pipeline result property.
    /// </summary>
	public class SetResult : CreateProcessor
	{
        /// <summary>
        /// Set Result after reading using current scope in Pipeline results
        /// </summary>
        /// <param name="arguments"></param>
		public override void Process(CreateArgs arguments)
		{
			Assert.ArgumentNotNull(arguments, "arguments");
			if (arguments.Result != null)
			{
				return;
			}
			ReadArgs readArgs = new ReadArgs(arguments.Scope);
			CorePipeline.Run("itemWebApiRead", readArgs);
			arguments.Result = readArgs.Result;
		}
	}
}
