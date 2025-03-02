using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;

public class InvoicePdfLinuxGenerator
{
    #region
    public MemoryStream GenerateTSInvoicePdf(InvoiceInfo invoiceInfo)
    {
        //    try
        //    {
        // Create a MemoryStream to hold the PDF
        MemoryStream memoryStream = new();

        // Create a PdfWriter instance to write to the MemoryStream
        PdfWriter writer = new PdfWriter(memoryStream);

        // Create a PdfDocument instance
        PdfDocument pdfDocument = new PdfDocument(writer);

        // Set the page size to A4
        PageSize pageSize = PageSize.A4;

        // Create a Document object to add content to the PDF
        Document doc = new Document(pdfDocument, pageSize);
        doc.SetMargins(50, 50, 50, 50); // Top, Right, Bottom, Left margins

        try
        {
            // Add a logo to the document
            string logoPath = "../../assets/logoonly.png"; // Update with actual path
            ImageData logoData = ImageDataFactory.Create(logoPath);
            Image logo = new Image(logoData)
                .SetFixedPosition(50, pdfDocument.GetDefaultPageSize().GetHeight() - 120)
                .ScaleToFit(80f, 80f);

            doc.Add(logo);
        }
        catch (Exception ex) { }

        // Initialize fonts
        PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont subHeaderFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        PdfFont helveticaItalicFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE);

        PdfFont billToLabelFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont billToValueFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont tableValueFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont bankDetailsFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        float tableFontSize = 8f;

        // Create a table with two columns
        Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
        headerTable.SetBorder(Border.NO_BORDER);

        // Add the logo to the first cell
        // headerTable.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE));

        // Add the company details to the second cell
        Paragraph companyDetails = new Paragraph()
            .Add("Signature Mail International Limited\n")
            .Add("信邮国际\n")
            .Add("No. 21, Jalan Sri Petaling 14, Sri Petaling, 57000 Kuala Lumpur, Malaysia\n")
            .Add("Tel: +603-9054 8502")
            .SetTextAlignment(TextAlignment.LEFT)
            .SetFontSize(10);

        headerTable.AddCell(new Cell().Add(companyDetails).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE));

        // Add the header table to the document
        doc.Add(headerTable);

        // Add header text
        Paragraph header = new Paragraph("Signature Mail International Limited")
            .SetFont(headerFont)
            .SetFontSize(14)
            .SetBold()
            .SetBorder(Border.NO_BORDER)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        doc.Add(header);

        try
        {
            // Add an additional image
            string imagePath = "../../assets/chinese_name.png"; // Update with actual path
            ImageData additionalImageData = ImageDataFactory.Create(imagePath);
            Image additionalImage = new Image(additionalImageData)
                .ScaleToFit(50f, 50f)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);
            doc.Add(additionalImage);
        }
        catch (Exception ex) { }

        // Add address and contact details
        Paragraph address = new Paragraph("No. 21, Jalan Sri Petaling 14, Sri Petaling 57000 Kuala Lumpur, Malaysia")
            .SetFont(subHeaderFont)
            .SetTextAlignment(TextAlignment.CENTER);
        doc.Add(address);

        Paragraph contact = new Paragraph("Tel: +603-9054 6502")
            .SetFontSize(8)
            .SetTextAlignment(TextAlignment.CENTER);
        doc.Add(contact);

        // Add a solid separator line
        ILineDrawer solidLine = new SolidLine(1); // 1pt thickness
                                                  // Add a separator line above the "INVOICE" title
        LineSeparator lineSeparator = new LineSeparator(solidLine).SetMarginTop(10).SetMarginBottom(10);

        doc.Add(lineSeparator);

        // Add title
        Paragraph title = new Paragraph("INVOICE")
            .SetFontSize(16)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER)
            .SetUnderline(1, 0);

        doc.Add(title);

        // Add some spacing
        doc.Add(new Paragraph("\n"));

        // Main table with 2 columns
        Table parentTable = new Table(UnitValue.CreatePercentArray(new float[] { 50f, 50f })).UseAllAvailableWidth();

        // Define the line spacing
        float top1_Spacing = 1f;

        // Create the first nested table (for the first column)
        Table billToInfo = new Table(UnitValue.CreatePercentArray(new float[] { 80f })).UseAllAvailableWidth();
        billToInfo.AddCell(new Cell().Add(new Paragraph("BILL TO:")
            .SetFont(billToLabelFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(80f)
            .SetBorder(Border.NO_BORDER));
        billToInfo.AddCell(new Cell().Add(new Paragraph("")
            .SetFont(billToValueFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(80f)
            .SetBorder(Border.NO_BORDER));
        billToInfo.AddCell(new Cell().Add(new Paragraph(invoiceInfo.BillTo)
            .SetFont(billToValueFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(80f)
            .SetBorder(Border.NO_BORDER));

        // Add the nested table to the first column of the main table
        parentTable.AddCell(new Cell().Add(billToInfo).SetPadding(5).SetBorder(Border.NO_BORDER));

        // Create the second nested table (for the second column)
        Table invoiceDetailsInfo = new Table(UnitValue.CreatePercentArray(new float[] { 30f, 70f })).UseAllAvailableWidth();
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph("Invoice No.:")
            .SetFont(billToLabelFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(30f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph(invoiceInfo.InvoiceNo)
            .SetFont(billToValueFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(70f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph("Invoice Date:")
            .SetFont(billToLabelFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(70f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph(invoiceInfo.InvoiceDate)
            .SetFont(billToValueFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(80f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph("Dispatch No.:")
            .SetFont(billToLabelFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(70f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));

        // Handle dispatches display, joining them with commas
        string dispatchesString = string.Join(",", invoiceInfo.Dispatches);
        invoiceDetailsInfo.AddCell(new Cell().Add(new Paragraph(dispatchesString)
            .SetFont(billToValueFont).SetFontSize(tableFontSize).SetMultipliedLeading(top1_Spacing))
            .SetMaxWidth(70f)
            .SetPadding(5)
            .SetBorder(Border.NO_BORDER));

        // Add the nested table to the second column of the main table
        parentTable.AddCell(new Cell().Add(invoiceDetailsInfo).SetPadding(5).SetBorder(Border.NO_BORDER));

        // Add the parent table to the document
        doc.Add(parentTable);

        // Add some spacing before the next section
        doc.Add(new Paragraph("\n"));
        // }


        // Add tables for each currency
        //    foreach (var currencyItem in invoiceInfo.CurrencyItem)
        //    {
        //        var product_items = currencyItem.Items.DistinctBy(x => x.ProductCode).ToList();
        //        var product_codes = product_items
        //                                .Select(x => x.ProductCode.ToUpper())
        //                                .Where(code => !string.IsNullOrEmpty(code)) // Filter out empty strings
        //                                .ToList();

        //        string product_code_string = string.Join(", ", product_codes);

        //        // Create the main table with 8 columns
        //        Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3f, 1f, 2f, 3f, 1f, 1f, 1f, 1f })).UseAllAvailableWidth();

        //        // First row
        //        table.AddCell(new Cell(2, 1).Add(new Paragraph("Dispatch No./Surcharge").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell(1, 4).Add(new Paragraph("PRT").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell(1, 2).Add(new Paragraph("EMS / REGISTERED / PRIME").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell(2, 1).Add(new Paragraph("Total Amount (USD)").SetBold()).SetTextAlignment(TextAlignment.CENTER));

        //        // Second row (Sub-headers for PRT and EMS sections)
        //        table.AddCell(new Cell().Add(new Paragraph("Weight (KG)").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell().Add(new Paragraph("Country").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell().Add(new Paragraph("Tracking No").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell().Add(new Paragraph("Rate/KG (USD)").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell().Add(new Paragraph("Quantity").SetBold()).SetTextAlignment(TextAlignment.CENTER));
        //        table.AddCell(new Cell().Add(new Paragraph("Unit Price (USD)").SetBold()).SetTextAlignment(TextAlignment.CENTER));

        //        // Add the items for the current currency
        //        foreach (var item in currencyItem.Items)
        //        {
        //            table.AddCell(new Cell(new Paragraph(item.DispatchNo, tableValueFont))
        //                            .SetTextAlignment(TextAlignment.CENTER)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Weight.ToString("N3") == "0.000" ? "" : item.Weight.ToString("N3"), tableValueFont))
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Country, tableValueFont))
        //                            .SetTextAlignment(TextAlignment.CENTER)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Identifier, tableValueFont))
        //                            .SetTextAlignment(TextAlignment.CENTER)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Rate.ToString("N2") == "0.00" ? "" : item.Rate.ToString("N2"), tableValueFont))
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Quantity.ToString() == "0" ? "" : item.Quantity.ToString(), tableValueFont))
        //                            .SetTextAlignment(TextAlignment.CENTER)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.UnitPrice.ToString("N2") == "0.00" ? "" : item.UnitPrice.ToString("N2"), tableValueFont))
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //            table.AddCell(new Cell(new Paragraph(item.Amount.ToString("N2") == "0.00" ? "" : item.Amount.ToString("N2"), tableValueFont))
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
        //        }

        //        table.AddCell(new Cell(1, 7).Add(new Paragraph("Dispatch No./Surcharge").SetBold()). 
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));

        //        table.AddCell(new Cell(1, 8).Add(new Paragraph(currencyItem.TotalAmount.ToString("N2")).SetBold()). 
        //                            .SetTextAlignment(TextAlignment.ALIGN_RIGHT)
        //                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));

        //        // Add the table to the document
        //        doc.Add(table);

        //        // Add some spacing before the next section
        //        doc.Add(new Paragraph("\n"));
        //    }

        // Add Bank Account Details with no left spacing
        IBlockElement bankDetailsTitle = new Paragraph("Bank Account Details")
            .SetFont(subHeaderFont)
            .SetBold()
            .SetFontSize(12);

        // Add some spacing before the next section
        doc.Add(new Paragraph("\n"));

        doc.Add(bankDetailsTitle);

        // Define the column widths (relative or fixed values)
        float[] columnWidths = { 2f, 8f }; // First column is 2x wider than the second column

        Table bankTable = new Table(UnitValue.CreatePercentArray(columnWidths));
        bankTable.SetWidth(UnitValue.CreatePercentValue(100)); // Table width is 100% of the page

        // Set the table border to NO_BORDER
        bankTable.SetBorder(Border.NO_BORDER);

        // Define the line spacing 
        float bankLineSpacing = 0.75f;

        // Add cells to the table with no borders 
        // Set line spacing using SetMultipliedLeading
        bankTable.AddCell(new Cell().Add(new Paragraph("Bank Name").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph(": OCBC Wing Hang Bank Limited").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph("Bank Address").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph(": 161 Queen's Road Central, Hong Kong").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph("Company Name").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph(": Signature Mail International Ltd").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph("Account Number").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph(": 035-805-461502831").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph("Swift Code").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));
        bankTable.AddCell(new Cell().Add(new Paragraph(": WIHBHKHH").SetFont(subHeaderFont).SetMultipliedLeading(bankLineSpacing)).SetBorder(Border.NO_BORDER));

        doc.Add(bankTable);

        // Add final notes
        IBlockElement note = new Paragraph("\nThank you for your business!\n\nThis is an auto-generated invoice, no signature is required.")
            .SetFont(helveticaItalicFont);
        doc.Add(note);

        try
        {
            // Add the stamp image
            string stampImagePath = "../../assets/SMIStamp.png"; // Update with actual path
            ImageData stampImageData = ImageDataFactory.Create(stampImagePath);
            Image stamp = new Image(stampImageData)
                .ScaleToFit(50f, 50f)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
            doc.Add(stamp);
        }
        catch (Exception ex) { }


        // Close the document
        doc.Close();

        // Optional: Reset the memory stream position
        memoryStream.Position = 0;

        return memoryStream;
        //    }   
        //    catch { }
    }
    #endregion

    public MemoryStream GenerateDEInvoicePdf(InvoiceInfo invoiceInfo)
    {
        // try
        // {
        // Create a MemoryStream to hold the PDF
        MemoryStream memoryStream = new();

        // Create a PdfWriter instance to write to the MemoryStream
        PdfWriter writer = new PdfWriter(memoryStream);

        // Create a PdfDocument instance
        PdfDocument pdfDocument = new PdfDocument(writer);

        // Set the page size to A4
        PageSize pageSize = PageSize.A4;

        // Create a Document object to add content to the PDF
        Document doc = new Document(pdfDocument, pageSize);
        doc.SetMargins(50, 50, 50, 50); // Top, Right, Bottom, Left margins

        // Initialize fonts
        PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont subHeaderFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);


        PdfFont headerLabelFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont headerValueFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        PdfFont tableValueFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        float tableFontSize = 9f;

        // Create a table with a columns
        Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100f })).UseAllAvailableWidth();
        headerTable.SetBorder(Border.NO_BORDER);

        // Add the company details to the second cell
        Paragraph companyDetails = new Paragraph()
            .Add("Signature Mail International Limited\n")
            .SetFontSize(14)
            .SetBold()
            .SetFont(headerFont);

        headerTable.AddCell(new Cell().Add(companyDetails)
           .SetBorder(Border.NO_BORDER)
           .SetTextAlignment(TextAlignment.CENTER)
           .SetVerticalAlignment(VerticalAlignment.MIDDLE));

        Paragraph companyAddress = new Paragraph()
            .Add("A-3A-2 Seri Gembira Avenue, Jalan Senang Ria, Happy Garden, 58200 Kuala Lumpur, Malaysia.\n")
            .SetFontSize(10)
            .SetFont(subHeaderFont);

        headerTable.AddCell(new Cell().Add(companyAddress)
           .SetBorder(Border.NO_BORDER)
           .SetTextAlignment(TextAlignment.CENTER)
           .SetVerticalAlignment(VerticalAlignment.MIDDLE));

        // Add the header table to the document
        doc.Add(headerTable);

        // Add a solid separator line
        ILineDrawer solidLine = new SolidLine(1); // 1pt thickness
                                                  // Add a separator line above the "INVOICE" title
        LineSeparator lineSeparator = new LineSeparator(solidLine).SetMarginTop(5).SetMarginBottom(5);

        doc.Add(lineSeparator);

        // Add title
        Paragraph title = new Paragraph("Commercial Invoice")
            .SetFontSize(16)
            .SetBold()
            .SetUnderline(1, -3)
            .SetTextAlignment(TextAlignment.CENTER);

        doc.Add(title);

        // Add some spacing
        doc.Add(new Paragraph("\n"));

        // Add consignee and invoice information
        Table infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 1.5f, 4f, 2.5f, 2f })).UseAllAvailableWidth();
        infoTable.SetBorder(Border.NO_BORDER);

        // First Row
        infoTable.AddCell(createCell("CONSIGNEE:", TextAlignment.LEFT, true).SetBorder(Border.NO_BORDER).SetFont(headerLabelFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("Saudi Pest Corporation", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER).SetFont(headerValueFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("INVOICE NO:", TextAlignment.LEFT, true).SetBorder(Border.NO_BORDER).SetFont(headerLabelFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("SMI202501734", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER).SetFont(headerValueFont).SetFontSize(tableFontSize));

        // Second Row
        infoTable.AddCell(createCell("ADDRESS:", TextAlignment.LEFT, true).SetBorder(Border.NO_BORDER).SetFont(headerLabelFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("8228 King Abdul Aziz Road Al Amal Riyadh 12643", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER).SetFont(headerValueFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("DATE:", TextAlignment.LEFT, true).SetBorder(Border.NO_BORDER).SetFont(headerLabelFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("24/01/2025", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER).SetFont(headerValueFont).SetFontSize(tableFontSize));

        // Third Row
        infoTable.AddCell(createCell("", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER));  // Empty cell for spacing
        infoTable.AddCell(createCell("", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER)); // Empty cell for spacing
        infoTable.AddCell(createCell("TOTAL CARTON:", TextAlignment.LEFT, true).SetBorder(Border.NO_BORDER).SetFont(headerLabelFont).SetFontSize(tableFontSize));
        infoTable.AddCell(createCell("7 CTNS", TextAlignment.LEFT, false).SetBorder(Border.NO_BORDER).SetFont(headerValueFont).SetFontSize(tableFontSize));

        doc.Add(infoTable);

        // Add some spacing
        doc.Add(new Paragraph("\n"));

        // Add table header
        Table dataTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 1, 2, 2 })).UseAllAvailableWidth();

        float dataTableFontSize = 8f;
        // Add headers
        dataTable.AddHeaderCell(createCell("No.", TextAlignment.CENTER, true).SetFontSize(dataTableFontSize));
        dataTable.AddHeaderCell(createCell("Description", TextAlignment.CENTER, true).SetFontSize(dataTableFontSize));
        dataTable.AddHeaderCell(createCell("Qty (Pcs)", TextAlignment.CENTER, true).SetFontSize(dataTableFontSize));
        dataTable.AddHeaderCell(createCell("Unit Price (SAR)", TextAlignment.CENTER, true).SetFontSize(dataTableFontSize));
        dataTable.AddHeaderCell(createCell("Total Price (SAR)", TextAlignment.CENTER, true).SetFontSize(dataTableFontSize));

        // Add rows
        dataTable.AddCell(createCell("1", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("battery", TextAlignment.LEFT, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("1", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("54.15", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("54.15", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));

        dataTable.AddCell(createCell("2", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("lithium battery", TextAlignment.LEFT, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("1", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("15.00", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));
        dataTable.AddCell(createCell("15.00", TextAlignment.CENTER, false).SetFontSize(dataTableFontSize));

        doc.Add(dataTable);

        try
        {
            // Add the stamp image
            string stampImagePath = "../../assets/SMIStamp.png"; // Update with actual path
            ImageData stampImageData = ImageDataFactory.Create(stampImagePath);
            Image stamp = new Image(stampImageData)
                .ScaleToFit(50f, 50f)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);
            doc.Add(stamp);
        }
        catch (Exception ex) { }


        // Close the document
        doc.Close();

        // Optional: Reset the memory stream position
        memoryStream.Position = 0;

        return memoryStream;

        // }
        // catch { }
    }

    private static Cell createCell(System.String content, TextAlignment alignment, System.Boolean bold)
    {
        Paragraph paragraph = new Paragraph(content);
        if (bold)
        {
            paragraph.SetBold();
        }
        return new Cell().Add(paragraph).SetTextAlignment(alignment).SetPadding(1);
    }

}

