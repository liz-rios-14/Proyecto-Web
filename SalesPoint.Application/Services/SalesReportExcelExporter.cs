using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using SalesPoint.Application.DTOs.Reports;

namespace SalesPoint.Application.Services;

internal static class SalesReportExcelExporter
{
    private const string SpreadsheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static byte[] Create(
        SalesReportDto report,
        DateTime startDate,
        DateTime endDate)
    {
        var sheets = BuildSheets(report, startDate, endDate);
        using var output = new MemoryStream();

        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            WriteText(archive, "[Content_Types].xml", BuildContentTypes(sheets.Count));
            WriteText(archive, "_rels/.rels", PackageRelationships);
            WriteText(archive, "xl/workbook.xml", BuildWorkbook(sheets));
            WriteText(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationships(sheets.Count));
            WriteText(archive, "xl/styles.xml", Styles);

            for (var index = 0; index < sheets.Count; index++)
            {
                WriteText(
                    archive,
                    $"xl/worksheets/sheet{index + 1}.xml",
                    BuildWorksheet(sheets[index]));
            }
        }

        return output.ToArray();
    }

    private static List<ExcelSheet> BuildSheets(
        SalesReportDto report,
        DateTime startDate,
        DateTime endDate)
    {
        var summary = new ExcelSheet(
            "Resumen",
            ["Concepto", "Valor"],
            [
                ["Fecha inicio", startDate],
                ["Fecha fin", endDate],
                ["Facturas", report.Totals.InvoiceCount],
                ["Subtotal", report.Totals.Subtotal],
                ["IVA", report.Totals.Tax],
                ["Total", report.Totals.Total]
            ],
            [22d, 18d]);

        var salesByDate = new ExcelSheet(
            "Ventas por fecha",
            ["Fecha", "Facturas", "Subtotal", "IVA", "Total"],
            report.SalesByDate
                .Select(item => new object?[]
                {
                    item.Date,
                    item.InvoiceCount,
                    item.Subtotal,
                    item.Tax,
                    item.Total
                })
                .ToList(),
            [16d, 12d, 16d, 16d, 16d]);

        var salesBySeller = new ExcelSheet(
            "Ventas por vendedor",
            ["Vendedor", "Facturas", "Total"],
            report.SalesBySeller
                .Select(item => new object?[]
                {
                    item.SellerName,
                    item.InvoiceCount,
                    item.Total
                })
                .ToList(),
            [32d, 12d, 16d]);

        var products = new ExcelSheet(
            "Productos vendidos",
            ["Producto", "Cantidad", "Total"],
            report.TopProducts
                .Select(item => new object?[]
                {
                    item.ProductName,
                    item.Quantity,
                    item.Total
                })
                .ToList(),
            [34d, 14d, 16d]);

        var lowStock = new ExcelSheet(
            "Stock bajo",
            ["Producto", "Stock"],
            report.LowStockProducts
                .Select(item => new object?[]
                {
                    item.ProductName,
                    item.Stock
                })
                .ToList(),
            [34d, 14d]);

        var customers = new ExcelSheet(
            "Clientes",
            ["Cliente", "Facturas", "Total"],
            report.TopCustomers
                .Select(item => new object?[]
                {
                    item.CustomerName,
                    item.InvoiceCount,
                    item.Total
                })
                .ToList(),
            [34d, 14d, 16d]);

        return [summary, salesByDate, salesBySeller, products, lowStock, customers];
    }

    private static string BuildWorksheet(ExcelSheet sheet)
    {
        var builder = new StringBuilder();
        using var writer = CreateWriter(builder);

        writer.WriteStartDocument();
        writer.WriteStartElement("worksheet", SpreadsheetNamespace);

        writer.WriteStartElement("sheetViews", SpreadsheetNamespace);
        writer.WriteStartElement("sheetView", SpreadsheetNamespace);
        writer.WriteAttributeString("workbookViewId", "0");
        writer.WriteStartElement("pane", SpreadsheetNamespace);
        writer.WriteAttributeString("ySplit", "1");
        writer.WriteAttributeString("topLeftCell", "A2");
        writer.WriteAttributeString("activePane", "bottomLeft");
        writer.WriteAttributeString("state", "frozen");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("cols", SpreadsheetNamespace);
        for (var index = 0; index < sheet.ColumnWidths.Count; index++)
        {
            writer.WriteStartElement("col", SpreadsheetNamespace);
            writer.WriteAttributeString("min", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("max", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("width", sheet.ColumnWidths[index].ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("customWidth", "1");
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("sheetData", SpreadsheetNamespace);
        WriteRow(writer, 1, sheet.Headers.Cast<object?>().ToArray(), true);

        for (var index = 0; index < sheet.Rows.Count; index++)
            WriteRow(writer, index + 2, sheet.Rows[index], false);

        writer.WriteEndElement();

        var lastColumn = GetColumnName(sheet.Headers.Count);
        writer.WriteStartElement("autoFilter", SpreadsheetNamespace);
        writer.WriteAttributeString("ref", $"A1:{lastColumn}{Math.Max(sheet.Rows.Count + 1, 1)}");
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        return builder.ToString();
    }

    private static void WriteRow(
        XmlWriter writer,
        int rowNumber,
        IReadOnlyList<object?> values,
        bool header)
    {
        writer.WriteStartElement("row", SpreadsheetNamespace);
        writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));

        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            writer.WriteStartElement("c", SpreadsheetNamespace);
            writer.WriteAttributeString("r", $"{GetColumnName(index + 1)}{rowNumber}");

            if (header)
            {
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteAttributeString("s", "1");
                WriteInlineString(writer, value?.ToString() ?? string.Empty);
            }
            else if (value is DateTime date)
            {
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteAttributeString("s", "2");
                WriteInlineString(writer, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            else if (value is decimal or double or float)
            {
                writer.WriteAttributeString("s", "3");
                writer.WriteElementString(
                    "v",
                    SpreadsheetNamespace,
                    Convert.ToDecimal(value, CultureInfo.InvariantCulture)
                        .ToString(CultureInfo.InvariantCulture));
            }
            else if (value is byte or short or int or long)
            {
                writer.WriteElementString(
                    "v",
                    SpreadsheetNamespace,
                    Convert.ToInt64(value, CultureInfo.InvariantCulture)
                        .ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttributeString("t", "inlineStr");
                WriteInlineString(writer, value?.ToString() ?? string.Empty);
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteInlineString(XmlWriter writer, string value)
    {
        writer.WriteStartElement("is", SpreadsheetNamespace);
        writer.WriteElementString("t", SpreadsheetNamespace, value);
        writer.WriteEndElement();
    }

    private static string BuildWorkbook(IReadOnlyList<ExcelSheet> sheets)
    {
        var builder = new StringBuilder();
        using var writer = CreateWriter(builder);

        writer.WriteStartDocument();
        writer.WriteStartElement("workbook", SpreadsheetNamespace);
        writer.WriteAttributeString(
            "xmlns",
            "r",
            null,
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
        writer.WriteStartElement("sheets", SpreadsheetNamespace);

        for (var index = 0; index < sheets.Count; index++)
        {
            writer.WriteStartElement("sheet", SpreadsheetNamespace);
            writer.WriteAttributeString("name", sheets[index].Name);
            writer.WriteAttributeString("sheetId", (index + 1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(
                "r",
                "id",
                "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
                $"rId{index + 1}");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Flush();
        return builder.ToString();
    }

    private static string BuildContentTypes(int sheetCount)
    {
        var overrides = string.Join(
            string.Empty,
            Enumerable.Range(1, sheetCount).Select(index =>
                $"<Override PartName=\"/xl/worksheets/sheet{index}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"));

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
              <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
              <Default Extension="xml" ContentType="application/xml"/>
              <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
              <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
              {{overrides}}
            </Types>
            """;
    }

    private static string BuildWorkbookRelationships(int sheetCount)
    {
        var relationships = string.Join(
            string.Empty,
            Enumerable.Range(1, sheetCount).Select(index =>
                $"<Relationship Id=\"rId{index}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{index}.xml\"/>"));

        return $$"""
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              {{relationships}}
              <Relationship Id="rId{{sheetCount + 1}}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
            </Relationships>
            """;
    }

    private static XmlWriter CreateWriter(StringBuilder builder) =>
        XmlWriter.Create(
            new Utf8StringWriter(builder),
            new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = false,
                OmitXmlDeclaration = false
            });

    private static void WriteText(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string GetColumnName(int number)
    {
        var result = string.Empty;
        while (number > 0)
        {
            number--;
            result = (char)('A' + number % 26) + result;
            number /= 26;
        }

        return result;
    }

    private const string PackageRelationships = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
          <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
        </Relationships>
        """;

    private const string Styles = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
          <fonts count="2">
            <font><sz val="11"/><name val="Calibri"/></font>
            <font><b/><color rgb="FFFFFFFF"/><sz val="11"/><name val="Calibri"/></font>
          </fonts>
          <fills count="3">
            <fill><patternFill patternType="none"/></fill>
            <fill><patternFill patternType="gray125"/></fill>
            <fill><patternFill patternType="solid"><fgColor rgb="FF1D4ED8"/><bgColor indexed="64"/></patternFill></fill>
          </fills>
          <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
          <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
          <cellXfs count="4">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"/>
            <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
            <xf numFmtId="4" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>
          </cellXfs>
          <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
        </styleSheet>
        """;

    private sealed record ExcelSheet(
        string Name,
        IReadOnlyList<string> Headers,
        IReadOnlyList<object?[]> Rows,
        IReadOnlyList<double> ColumnWidths);

    private sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder builder)
            : base(builder, CultureInfo.InvariantCulture)
        {
        }

        public override Encoding Encoding => new UTF8Encoding(false);
    }
}
