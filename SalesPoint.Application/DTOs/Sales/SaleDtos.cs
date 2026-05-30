namespace SalesPoint.Application.DTOs.Sales;

public sealed class SaleCreateDto
{
    public int CustomerId { get; set; }
    public List<SaleDetailCreateDto> Details { get; set; } = new();
}

public sealed class SaleDetailCreateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class SaleDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<SaleDetailDto> Details { get; set; } = new();
}

public sealed class SaleDetailDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class SaleHistoryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
