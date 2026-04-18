using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.API_Clean_Architecture.Filters {
	public class FileUploadOperationFilter : IOperationFilter {
		public void Apply(OpenApiOperation operation, OperationFilterContext context) {
			ParameterInfo[] fileParams = context.MethodInfo.GetParameters()
				.Where(p => p.ParameterType == typeof(IFormFile)).ToArray();

			if (fileParams.Length <= 0) {
				return;
			}

			operation.RequestBody = new OpenApiRequestBody {
				Content = new Dictionary<string, OpenApiMediaType> {
					["multipart/form-data"] = new OpenApiMediaType {
						Schema = new OpenApiSchema {
							Type = JsonSchemaType.Object,
							Properties = new Dictionary<string, IOpenApiSchema> {
								["imagen"] = new OpenApiSchema {
									Type = JsonSchemaType.String,
									Format = "binary"
								},
								["fileName"] = new OpenApiSchema {
									Type = JsonSchemaType.String,
									Description = "Name"
								}
							}
						}
					}
				}
			};
		}
	}
}