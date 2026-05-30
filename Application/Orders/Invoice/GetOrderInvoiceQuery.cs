using MediatR;

namespace ColeccionaloYa.Application.Orders.Invoice;

public record InvoiceFile(byte[] Content, string FileName);

public record GetOrderInvoiceQuery(int OrderId, int RequesterId, bool IsAdmin) : IRequest<InvoiceFile>;
