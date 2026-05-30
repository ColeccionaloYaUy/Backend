using ColeccionaloYa.Application.Authors;
using ColeccionaloYa.Application.Authors.CreateAuthor;
using ColeccionaloYa.Application.Authors.DeleteAuthor;
using ColeccionaloYa.Application.Authors.GetAuthorById;
using ColeccionaloYa.Application.Authors.GetAuthors;
using ColeccionaloYa.Application.Authors.UpdateAuthor;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Authors;

[Route("api/[controller]")]
[ApiController]
public class AuthorsController : ControllerBase {
	private readonly IMediator _Mediator;

	public AuthorsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<PagedResult<AuthorDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery] string? search = null,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20
	) =>
		_Mediator.Send(new GetAuthorsQuery(page, limit, search), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<AuthorDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetAuthorByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] AuthorRequest request, CancellationToken cancellationToken) {
		var author = await _Mediator.Send(new CreateAuthorCommand(request.Name), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<AuthorDto> Update(int id, [FromBody] AuthorRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateAuthorCommand(id, request.Name), cancellationToken);

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteAuthorCommand(id), cancellationToken);
		return NoContent();
	}
}
