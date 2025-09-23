using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EXAMPLE.API
{
    public class CleanTagNamesFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Clean global tags
            if (swaggerDoc.Tags != null)
            {
                foreach (var tag in swaggerDoc.Tags)
                {
                    tag.Name = CleanName(tag.Name);
                }
            }

            // Clean operation tags (important!)
            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    for (int i = 0; i < operation.Value.Tags.Count; i++)
                    {
                        operation.Value.Tags[i].Name = CleanName(operation.Value.Tags[i].Name);
                    }
                }
            }
        }

        private string CleanName(string name)
        {
            var dashIndex = name.IndexOf('-');
            return dashIndex > 0 ? name.Substring(dashIndex + 1) : name;
        }
    }
}