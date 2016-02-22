using Elision.Entities.ParameterFields.Design;
using Elision.Entities.ParameterFields.Theme;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Elision.Entities.RenderingParameters.Structure
{
    public class StructureRenderingParameters : IClassParameters, IColorSchemeParameters
    {
        public ID RenderingUniqueId { get; set; }

        public string CssClass { get; set; }
        public Item ColorScheme { get; set; }
    }
}
