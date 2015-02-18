using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Read;
using Sitecore.Pipelines;
using System;
namespace Mindtree.ItemWebApi.Pipelines.Version.Create
{
	public class SetResult : CreateVersionProcessor
	{
		public override void Process(CreateVersionArgs arguments)
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