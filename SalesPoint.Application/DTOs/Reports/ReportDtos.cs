namespace SalesPoint.Application.DTOs.Reports;

public sealed class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class SalesReportDto
{
    public List<SalesByDateDto> SalesByDate { get; set; } = new();
    public List<SalesBySellerDto> SalesBySeller { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public ReportTotalsDto Totals { get; set; } = new();
}

public sealed class SalesByDateDto
{
    public DateTime Date { get; set; }
    public int InvoiceCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

public sealed class SalesBySellerDto
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal Total { get; set; }
}

public sealed class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}

public sealed class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public sealed class TopCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal Total { get; set; }
}

public sealed class ReportTotalsDto
{
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public int InvoiceCount { get; set; }
}
