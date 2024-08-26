using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

public class PdfGenerator
{
    public MemoryStream GenerateInvoicePdf(InvoiceInfo invoiceInfo)
    {
        // Create a MemoryStream to hold the PDF
        MemoryStream stream = new();

        // Create a Document object
        Document doc = new(PageSize.A4, 50, 50, 50, 50);

        // Create a PdfWriter instance to write to the MemoryStream
        PdfWriter writer = PdfWriter.GetInstance(doc, stream);

        writer.CloseStream = false;

        // Open the document to start writing
        doc.Open();

        // Add the logo as a background image
        string logoPath = "../../assets/logoonly.png"; // Update with actual path
        Image logo = Image.GetInstance(logoPath);
        logo.SetAbsolutePosition(50, doc.PageSize.Height - 120); // Adjust position as needed
        logo.ScaleToFit(80f, 80f);
        logo.Alignment = Image.UNDERLYING; // Position behind other elements
        doc.Add(logo);

        // Create a table with 1 column for the header text to span across the document
        PdfPTable headerTable = new PdfPTable(1)
        {
            WidthPercentage = 100
        };

        // Add the header text and center align it
        Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
        Font subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

        PdfPCell textCell = new PdfPCell
        {
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = Element.ALIGN_CENTER, // Center-align the text
            VerticalAlignment = Element.ALIGN_MIDDLE
        };

        textCell.AddElement(new Paragraph("Signature Mail International Limited", headerFont) { Alignment = Element.ALIGN_CENTER });

        // Add the image in between the text
        string imagePath = "../../assets/chinese_name.png"; // Update with actual path
        Image additionalImage = Image.GetInstance(imagePath);
        additionalImage.ScaleToFit(50f, 50f); // Adjust the size as needed
        additionalImage.Alignment = Element.ALIGN_CENTER; // Center-align the image
        textCell.AddElement(additionalImage);

        textCell.AddElement(new Paragraph("No. 21, Jalan Sri Petaling 14 , Sri Petaling 57000 Kuala Lumpur, Malaysia", subHeaderFont) { Alignment = Element.ALIGN_CENTER });
        textCell.AddElement(new Paragraph("Tel: +603-9054 6502", subHeaderFont) { Alignment = Element.ALIGN_CENTER });

        headerTable.AddCell(textCell);

        // Add the header table to the document
        doc.Add(headerTable);

        // Add some spacing before the next section
        doc.Add(new Paragraph("\n"));

        // Add a separator line above the "INVOICE" title
        LineSeparator separator = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2);
        doc.Add(separator);

        // Add the title
        Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, Font.UNDERLINE);
        Paragraph title = new("INVOICE", titleFont)
        {
            Alignment = Element.ALIGN_CENTER
        };
        doc.Add(title);

        // Add some spacing before the next section
        doc.Add(new Paragraph("\n"));

        // Create a table with 2 columns for side-by-side layout
        PdfPTable billToAndDetailsTable = new(2)
        {
            WidthPercentage = 100
        };
        billToAndDetailsTable.SetWidths(new float[] { 1, 1 }); // Set column widths to be equal

        // Initialize Fonts
        Font billToLabel_Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
        Font billToValue_Font = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        Font tableValue_Font = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        Font bankDetails_Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

        // Add the Bill To Section in the first column
        PdfPCell billToCell = new()
        {
            Border = Rectangle.NO_BORDER
        };

        billToCell.AddElement(new Paragraph("BILL TO:", billToLabel_Font));
        billToCell.AddElement(new Paragraph(invoiceInfo.BillTo, billToValue_Font));
        billToAndDetailsTable.AddCell(billToCell);

        // Add the Invoice Details in the second column
        PdfPCell detailsCell = new()
        {
            Border = Rectangle.NO_BORDER
        };

        PdfPTable invoiceDetailsTable = new(2);
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase("Invoice No.:", billToLabel_Font)) { Border = Rectangle.NO_BORDER });
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase(invoiceInfo.InvoiceNo, billToValue_Font)) { Border = Rectangle.NO_BORDER });
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase("Invoice Date:", billToLabel_Font)) { Border = Rectangle.NO_BORDER });
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase(invoiceInfo.InvoiceDate, billToValue_Font)) { Border = Rectangle.NO_BORDER });
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase("Dispatch No.:", billToLabel_Font)) { Border = Rectangle.NO_BORDER });

        // Handle dispatches display, joining them with commas
        string dispatchesString = string.Join(",", invoiceInfo.Dispatches);
        invoiceDetailsTable.AddCell(new PdfPCell(new Phrase(dispatchesString, billToValue_Font)) { Border = Rectangle.NO_BORDER });

        detailsCell.AddElement(invoiceDetailsTable);
        billToAndDetailsTable.AddCell(detailsCell);

        // Add the table to the document
        doc.Add(billToAndDetailsTable);

        // Add some spacing before the next section
        doc.Add(new Paragraph("\n"));

        // Add tables for each currency
        foreach (var currencyItem in invoiceInfo.CurrencyItem)
        {
            var product_items = currencyItem.Items.GroupBy(x => x.ProductCode).ToList();
            // Add a subtitle for the currency
            // Paragraph currencyTitle = new Paragraph($"Currency: {currencyItem.Currency}", billToFont)
            // {
            //     SpacingBefore = 10f,
            //     SpacingAfter = 5f
            // };
            // doc.Add(currencyTitle);

            foreach (var group in product_items)
            {
                // Create a table with 8 columns
                PdfPTable table = new PdfPTable(8)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 3f, 1f, 2f, 3f, 1f, 1f, 1f, 1f });

                // First row
                PdfPCell cell1 = new PdfPCell(new Phrase("Dispatch No./Surcharge", tableValue_Font))
                {
                    Rowspan = 2,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                table.AddCell(cell1);

                PdfPCell cell2 = new PdfPCell(new Phrase($"{group.Key.ToUpper()}", tableValue_Font))
                {
                    Colspan = 4,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                table.AddCell(cell2);

                PdfPCell cell3 = new PdfPCell(new Phrase("EMS / REGISTERED / PRIME", tableValue_Font))
                {
                    Colspan = 2,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                table.AddCell(cell3);

                PdfPCell cell4 = new PdfPCell(new Phrase($"Total Amount ({currencyItem.Currency})", tableValue_Font))
                {
                    Rowspan = 2,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                table.AddCell(cell4);

                // Second row
                table.AddCell(new PdfPCell(new Phrase("Weight (KG)", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                table.AddCell(new PdfPCell(new Phrase("Country", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                table.AddCell(new PdfPCell(new Phrase("Tracking No", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                table.AddCell(new PdfPCell(new Phrase($"Rate /KG ({currencyItem.Currency})", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                table.AddCell(new PdfPCell(new Phrase("Quantity", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                });

                table.AddCell(new PdfPCell(new Phrase($"Unit Price ({currencyItem.Currency})", tableValue_Font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                });

                // Add the items for the current currency
                foreach (var item in group)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.DispatchNo, tableValue_Font)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Weight.ToString("N3") == "0.000" ? "" : item.Weight.ToString("N3"), tableValue_Font)) { HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Country, tableValue_Font)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Identifier, tableValue_Font)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("N2") == "0.00" ? "" : item.Rate.ToString("N2"), tableValue_Font)) { HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString() == "0" ? "" : item.Quantity.ToString(), tableValue_Font)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.UnitPrice.ToString("N2") == "0.00" ? "" : item.UnitPrice.ToString("N2"), tableValue_Font)) { HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                    table.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("N2") == "0.00" ? "" : item.Amount.ToString("N2"), tableValue_Font)) { HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE });
                }

                PdfPCell totalLabelCell = new PdfPCell(new Phrase("Total ", FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    Colspan = 7,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                };
                table.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase(currencyItem.TotalAmount.ToString("N2"), FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                };
                table.AddCell(totalValueCell);

                // Add the currency table to the document
                doc.Add(table);

                // Add some spacing before the next section
                doc.Add(new Paragraph("\n"));
            }
        }


        // Add Bank Account Details with no left spacing
        Paragraph bankDetailsTitle = new Paragraph("Bank Account Details", bankDetails_Font)
        {
            SpacingBefore = 10f
        };
        doc.Add(bankDetailsTitle);

        PdfPTable bankTable = new PdfPTable(2)
        {
            WidthPercentage = 100,
            SpacingBefore = 5f
        };
        bankTable.SetWidths(new float[] { 2f, 8f });
        bankTable.DefaultCell.Border = Rectangle.NO_BORDER;

        bankTable.AddCell(new PdfPCell(new Phrase("Bank Name", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase(": OCBC Wing Hang Bank Limited", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase("Bank Address", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase(": 161 Queen's Road Central, Hong Kong", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase("Company Name", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase(": Signature Mail International Ltd", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase("Account Number", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase(": 035-805-461502831", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase("Swift Code", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });
        bankTable.AddCell(new PdfPCell(new Phrase(": WIHBHKHH", tableValue_Font)) { Border = Rectangle.NO_BORDER, Padding = 0 });

        doc.Add(bankTable);

        // Add final notes
        Paragraph note = new("\nThank you for your business!\n\nThis is an auto-generated invoice, no signature is required.", FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC));
        doc.Add(note);

        // Add the stamp image
        string stampImagePath = "../../assets/SMIStamp.png"; // Update with actual path
        Image stamp = Image.GetInstance(stampImagePath);
        stamp.ScaleToFit(50f, 50f);
        stamp.Alignment = Image.ALIGN_RIGHT;
        doc.Add(stamp);

        // Close the document
        doc.Close();

        stream.Position = 0;

        return stream;
    }
}

