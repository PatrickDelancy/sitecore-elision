using System;
using System.Linq;
using Elision.Data;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetChromeData;

namespace Elision.Fragments
{
    public class GetFragmentRenderingChromeData : GetChromeDataProcessor
    {
        public override void Process(GetChromeDataArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNull(args.ChromeData, "Chrome Data");
            if (!"rendering".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
                return;

            if (args.Item == null)
                return;

            var renderingReference = args.CustomData["renderingReference"] as Sitecore.Layouts.RenderingReference;
            if (renderingReference == null || renderingReference.Settings == null)
                return;

            var fragmentId = StringUtil.ExtractParameter("fragmentid", renderingReference.Settings.Parameters);
            var wasfragment = "1" == StringUtil.ExtractParameter("wasfragment", renderingReference.Settings.Parameters);

            var datasourceItem = renderingReference.Database.ResolveDatasource(renderingReference.Settings.DataSource);
            var editFragmentCommand = args.ChromeData.Commands
                                          .FirstOrDefault(x => x.Click.Contains("deg:rendering:editfragment()"));
            if (editFragmentCommand != null)
            {
                if (string.IsNullOrWhiteSpace(fragmentId) || !wasfragment)
                {
                    args.ChromeData.Commands.Remove(editFragmentCommand);
                }
                else
                {
                    editFragmentCommand.Click = editFragmentCommand
                        .Click.Replace("deg:rendering:editfragment()",
                                       string.Format("deg:rendering:editfragment(fragmentid={0})", fragmentId));
                }
            }
        }
    }
}
