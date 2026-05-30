using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Books;

public class AddBookAuthorsRequest {
	[Required]
	[MinLength(1)]
	public List<int> AuthorIds { get; set; } = new();
}

public class AddBookGenresRequest {
	[Required]
	[MinLength(1)]
	public List<int> GenreIds { get; set; } = new();
}
