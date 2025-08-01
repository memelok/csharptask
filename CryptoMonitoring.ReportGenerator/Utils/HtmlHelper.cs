using RazorLight;
using Microsoft.EntityFrameworkCore;


namespace CryptoMonitoring.ReportGenerator.Utils
{
    public class HtmlHelper
    {
        public static Task<string> RenderAsync(
            IRazorLightEngine engine,
            string templateKey,
            object model)
        {
            return engine.CompileRenderAsync(templateKey, model);
        }
    }
}
