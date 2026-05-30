using ColeccionaloYa.Application.Books;
using ColeccionaloYa.Application.Books.AddBookAuthors;
using ColeccionaloYa.Application.Books.AddBookGenres;
using ColeccionaloYa.Application.Books.CreateBook;
using ColeccionaloYa.Application.Books.GetBookById;
using ColeccionaloYa.Application.Books.RemoveBookAuthor;
using ColeccionaloYa.Application.Books.RemoveBookGenre;
using ColeccionaloYa.Application.Books.UpdateBook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Books;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase {
	private readonly IMediator _Mediator;

	public BooksController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet("{id:int}")]
	public Task<BookDetailDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetBookByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateBookRequest request, CancellationToken cancellationToken) {
		var book = await _Mediator.Send(new CreateBookCommand(
			request.Name, request.ShortDescription, request.LongDescription, request.Price, request.Weight,
			request.TypeBook, request.AuthorIds, request.GenreIds, request.IsFeatured), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = book.IdProduct }, book);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<BookDetailDto> Update(int id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateBookCommand(
			id, request.Name, request.ShortDescription, request.LongDescription,
			request.Price, request.Weight, request.TypeBook), cancellationToken);

	[HttpPost("{id:int}/authors")]
	[Authorize(Roles = "Admin")]
	public Task<BookAuthorsDto> AddAuthors(int id, [FromBody] AddBookAuthorsRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AddBookAuthorsCommand(id, request.AuthorIds), cancellationToken);

	[HttpDelete("{id:int}/authors/{idAuthor:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveAuthor(int id, int idAuthor, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemoveBookAuthorCommand(id, idAuthor), cancellationToken);
		return NoContent();
	}

	[HttpPost("{id:int}/genres")]
	[Authorize(Roles = "Admin")]
	public Task<BookGenresDto> AddGenres(int id, [FromBody] AddBookGenresRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AddBookGenresCommand(id, request.GenreIds), cancellationToken);

	[HttpDelete("{id:int}/genres/{idGenre:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveGenre(int id, int idGenre, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemoveBookGenreCommand(id, idGenre), cancellationToken);
		return NoContent();
	}
}
