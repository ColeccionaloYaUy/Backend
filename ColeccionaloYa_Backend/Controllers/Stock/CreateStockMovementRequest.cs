using System.ComponentModel.DataAnnotations;

namespace ColeccionaloYa.API_Clean_Architecture.Controllers.Stock;

public class CreateStockMovementRequest {
	public int IdProduct { get; set; }

	[Required]
	public string Type { get; set; } = string.Empty;

	public int Quantity { get; set; }

	public DateTime Date { get; set; }

	public int? IdOrder { get; set; }

	[MaxLength(500)]
	public string? Reason { get; set; }
}
