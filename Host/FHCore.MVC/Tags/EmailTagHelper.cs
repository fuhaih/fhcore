using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;
namespace FHCore.MVC.Tags
{
    [HtmlTargetElement("email")] 
    public class EmailTagHelper:TagHelper
    {

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName="a";
        }
        
    }
}