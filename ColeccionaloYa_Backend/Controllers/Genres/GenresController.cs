using ColeccionaloYa.Application.Genres;
using ColeccionaloYa.Application.Genres.CreateGenre;
using ColeccionaloYa.Application.Genres.DeleteGenre;
using ColeccionaloYa.Application.Genres.GetGenreById;
using ColeccionaloYa.Application.Genres.GetGenres;
using ColeccionaloYa.Application.Genres.UpdateGenre;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Genres;

[Route("api/[controller]")]
[ApiController]
public class GenresController : ControllerBase {
	private readonly IMediator _Mediator;

	public GenresController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<GenreDto>> GetAll(CancellationToken cancellationToken) =>
		_Mediator.Send(new GetGenresQuery(), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<GenreDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetGenreByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] GenreRequest request, CancellationToken cancellationToken) {
		var genre = await _Mediator.Send(new CreateGenreCommand(request.Name), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = genre.Id }, genre);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<GenreDto> Update(int id, [FromBody] GenreRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateGenreCommand(id, request.Name), cancellationToken);

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteGenreCommand(id), cancellationToken);
		return NoContent();
	}
}
