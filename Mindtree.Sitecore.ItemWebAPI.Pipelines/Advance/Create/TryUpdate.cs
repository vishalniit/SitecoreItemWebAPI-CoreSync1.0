using Sitecore.Diagnostics;
using Sitecore.ItemWebApi.Pipelines.Update;
using Sitecore.Pipelines;
using System;
using System.Web;
namespace Mindtree.ItemWebApi.Pipelines.Advance.Create
{
    /// <summary>
    /// Update class to update item if required.
    /// </summary>
	public class TryUpdate : CreateProcessor
	{
        /// <summary>
        /// Function which update the Item after creating it successfully
        /// </summary>
        /// <param name="arguments"></param>
		public override void Process(CreateArgs arguments)
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
