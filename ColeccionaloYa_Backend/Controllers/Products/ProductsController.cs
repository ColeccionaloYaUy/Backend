using ColeccionaloYa.Application.Products;
using ColeccionaloYa.Application.Products.CreateProduct;
using ColeccionaloYa.Application.Products.DeleteProduct;
using ColeccionaloYa.Application.Products.GetProductById;
using ColeccionaloYa.Application.Products.GetProducts;
using ColeccionaloYa.Application.Products.ProductTags;
using ColeccionaloYa.Application.Products.ProductTags.AddProductTags;
using ColeccionaloYa.Application.Products.ProductTags.RemoveProductTag;
using ColeccionaloYa.Application.Products.UpdateProduct;
using ColeccionaloYa.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Products;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase {
	private readonly IMediator _Mediator;

	public ProductsController(IMediator mediator) {
		_Mediator = mediator;
	}

	[HttpGet]
	public Task<PagedResult<ProductSummaryDto>> GetAll(
		CancellationToken cancellationToken,
		[FromQuery] int page = 1,
		[FromQuery] int limit = 20,
		[FromQuery] string? search = null,
		[FromQuery] string? type = null,
		[FromQuery(Name = "type_book")] string? typeBook = null,
		[FromQuery] List<int>? tags = null,
		[FromQuery] List<int>? genres = null,
		[FromQuery] List<int>? authors = null,
		[FromQuery] int? franchise = null,
		[FromQuery(Name = "min_price")] decimal? minPrice = null,
		[FromQuery(Name = "max_price")] decimal? maxPrice = null,
		[FromQuery(Name = "in_stock")] bool? inStock = null,
		[FromQuery(Name = "on_sale")] bool? onSale = null,
		[FromQuery] bool? featured = null,
		[FromQuery(Name = "new_arrivals")] bool newArrivals = false,
		[FromQuery(Name = "related_to")] int? relatedTo = null,
		[FromQuery] string sort = "newest"
	) =>
		_Mediator.Send(new GetProductsQuery(
			page, limit, search, type, typeBook,
			tags ?? new List<int>(), genres ?? new List<int>(), authors ?? new List<int>(),
			franchise, minPrice, maxPrice, inStock, onSale, featured, newArrivals, relatedTo, sort),
			cancellationToken);

	[HttpGet("{id:int}")]
	public Task<ProductDetailDto> GetById(int id, CancellationToken cancellationToken) =>
		_Mediator.Send(new GetProductByIdQuery(id), cancellationToken);

	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken) {
		var product = await _Mediator.Send(new CreateProductCommand(
			request.Name, request.ShortDescription, request.LongDescription,
			request.Price, request.Type, request.Weight, request.IsFeatured), cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = product.IdProduct }, product);
	}

	[HttpPut("{id:int}")]
	[Authorize(Roles = "Admin")]
	public Task<ProductDto> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new UpdateProductCommand(
			id, request.Name, request.ShortDescription, request.LongDescription,
			request.Price, request.Type, request.Weight, request.IsFeatured), cancellationToken);

	[HttpDelete("{id:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) {
		await _Mediator.Send(new DeleteProductCommand(id), cancellationToken);
		return NoContent();
	}

	[HttpPost("{id:int}/tags")]
	[Authorize(Roles = "Admin")]
	public Task<ProductTagsDto> AddTags(int id, [FromBody] AddProductTagsRequest request, CancellationToken cancellationToken) =>
		_Mediator.Send(new AddProductTagsCommand(id, request.TagIds), cancellationToken);

	[HttpDelete("{id:int}/tags/{idTag:int}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveTag(int id, int idTag, CancellationToken cancellationToken) {
		await _Mediator.Send(new RemoveProductTagCommand(id, idTag), cancellationToken);
		return NoContent();
	}
}
