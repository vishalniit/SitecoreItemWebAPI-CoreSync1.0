using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Update;
using Sitecore.Pipelines;
using System;
using System.Web;
namespace Mindtree.ItemWebApi.Pipelines.Version.Create
{
    public class TryUpdate : CreateVersionProcessor
	{
		public override void Process(CreateVersionArgs arguments)
		{
			Assert.ArgumentNotNull(arguments, "arguments");
			System.Web.HttpRequest request = arguments.Context.HttpContext.Request;
			if (request.Form.AllKeys.Length == 0)
			{
				return;
			}
			UpdateArgs updateArgs = new UpdateArgs(arguments.Scope);
			CorePipeline.Run("itemWebApiUpdate", updateArgs);
			arguments.Result = updateArgs.Result;
		}
	}
}