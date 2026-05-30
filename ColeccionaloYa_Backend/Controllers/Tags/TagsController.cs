using ColeccionaloYa.Application.Tags;
using ColeccionaloYa.Application.Tags.CreateTag;
using ColeccionaloYa.Application.Tags.DeleteTag;
using ColeccionaloYa.Application.Tags.GetTagById;
using ColeccionaloYa.Application.Tags.GetTags;
using ColeccionaloYa.Application.Tags.UpdateTag;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Tags;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase {
	private readonly IMediator _Mediator;

	public TagsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<List<TagDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery(Name = "is_franchise")] bool? isFranchise = null,
		[FromQuery] string? search = null
	) =>
		_Mediator.Send(new GetTagsQuery(isFranchise, search), cancellationToken);

	[HttpGet("{id:int}")]
	public Task<TagDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetTagByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken) {
		var tag = await _Mediator.Send(new CreateTagCommand(request.Name, request.IsFranchise), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<TagDto> Update(int id, [FromBody] UpdateTagRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateTagCommand(id, request.Name, request.IsFranchise), cancellationToken);

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteTagCommand(id), cancellationToken);
		return NoContent();
	}
}
