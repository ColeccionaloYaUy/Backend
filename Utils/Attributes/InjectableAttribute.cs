using Microsoft.Extensions.DependencyInjection;

namespace API.Utils.Attributes {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class InjectableAttribute : Attribute {
		public ServiceLifetime Lifetime { get; }

		public InjectableAttribute(ServiceLifetime lifetime) {
			Lifetime = lifetime;
		}
	}
}
