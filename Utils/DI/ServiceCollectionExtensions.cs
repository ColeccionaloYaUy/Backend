using System.Reflection;
using System.Text.RegularExpressions;
using API.Utils.Attributes;
using API.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API.Utils.DI;

public static class ServiceCollectionExtensions {
	private const string SEARCH_PATTERN = "^API.*";

	public static void AddInjectables(this IHostApplicationBuilder builder) {
		var assemblies = new List<Assembly>();
		var dllFiles = AppDomain.CurrentDomain.GetProjectAssemblies();

		if (!dllFiles.Any()) {
			return;
		}

		foreach (var dllFile in dllFiles) {
			try {
				var assembly = Assembly.LoadFrom(dllFile);
				assemblies.Add(assembly);
			} catch (Exception) {
			}
		}

		ProcessAssemblies(builder.Services, assemblies);
	}

	private static void ProcessAssemblies(IServiceCollection services, IEnumerable<Assembly> assemblies) {
		var regex = new Regex(SEARCH_PATTERN, RegexOptions.IgnoreCase);

		foreach (var assembly in assemblies) {
			try {
				var typesWithAttribute = assembly.GetTypes()
					.Where(type => type.IsClass &&
									!type.IsAbstract &&
									type.GetCustomAttribute<InjectableAttribute>() != null);

				foreach (var type in typesWithAttribute) {
					var attribute = type.GetCustomAttribute<InjectableAttribute>();

					var interfaces = type.GetInterfaces()
						.Where(i => i.Namespace != null && regex.IsMatch(i.Namespace));

					if (interfaces.Any()) {
						foreach (var interfaceType in interfaces) {
							RegisterService(services, interfaceType, type, attribute.Lifetime);
						}
					} else {
						RegisterService(services, type, type, attribute.Lifetime);
					}
				}
			} catch (Exception) {
			}
		}
	}

	private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType,
		ServiceLifetime lifetime) {
		switch (lifetime) {
			case ServiceLifetime.Singleton:
				services.AddSingleton(serviceType, implementationType);
				break;
			case ServiceLifetime.Scoped:
				services.AddScoped(serviceType, implementationType);
				break;
			case ServiceLifetime.Transient:
				services.AddTransient(serviceType, implementationType);
				break;
		}
	}
}