using Microsoft.Extensions.DependencyInjection;

namespace ColeccionaloYa.Utils.Attributes {
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class InjectableAttribute : Attribute {
		public ServiceLifetime Lifetime { get; }

		public InjectableAttribute(ServiceLifetime lifetime) {
			Lifetime = lifetime;
		}
	}
}
