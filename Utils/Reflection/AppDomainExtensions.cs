namespace API.Utils.Reflection {
	public static class AppDomainExtensions {
		private const string SEARCH_PATTERN = "API.*.dll";

		public static string[]? GetProjectAssemblies(this AppDomain appDomain) {
			var baseDirectory = appDomain.BaseDirectory;

			if (!Directory.Exists(baseDirectory)) {
				return [];
			}

			var dllFiles = Directory.GetFiles(baseDirectory, SEARCH_PATTERN, SearchOption.AllDirectories);

			return dllFiles;
		}

	}
}
