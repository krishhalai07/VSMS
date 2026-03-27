using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VSMS
{
    public static class InvoicePdf
    {
        public static byte[] Generate(
            int bId, int? jId, int? partCost, int? gst, int? totalCost,
            string? status, string? paymentType,
            string ownerName, string ownerEmail, string ownerContact,
            string carName, string carModel, string carYear, string numberPlate,
            string problem, string mechName, string dateIn, string dateOut, string jobStatus,
            List<(string Name, int Price)> parts)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string statusColor = status == "Paid"    ? "#16a34a"
                               : status == "Overdue" ? "#dc2626"
                               : "#d97706";

            string billingDate = DateTime.Now.ToString("dd MMM yyyy");

            return Document.Create(doc =>
            {
                doc.Page(pg =>
                {
                    pg.Size(PageSizes.A4);
                    pg.MarginHorizontal(0);
                    pg.MarginVertical(0);
                    pg.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                    pg.Content().Column(col =>
                    {
                        // ── HEADER BANNER ────────────────────────
                        col.Item().Background("#0f172a").Padding(28).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(48).Height(48)
                                        .Background("#4338ca")
                                        .AlignCenter().AlignMiddle()
                                        .Text("🚗").FontSize(24);
                                    r.ConstantItem(12);
                                    r.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("VSMS")
                                            .Bold().FontSize(26).FontColor("#ffffff");
                                        inner.Item().Text("Vehicle Service Management System")
                                            .FontSize(8).FontColor("#94a3b8");
                                    });
                                });
                            });
                            row.ConstantItem(150).AlignRight().Column(c =>
                            {
                                c.Item().Background("#1e293b").Padding(12).Column(inner =>
                                {
                                    inner.Item().Text("INVOICE").Bold().FontSize(18)
                                        .FontColor("#ffffff").AlignRight();
                                    inner.Item().PaddingTop(3)
                                        .Text($"# INV-{bId:D4}").FontSize(11)
                                        .FontColor("#818cf8").AlignRight();
                                    inner.Item().PaddingTop(4)
                                        .Text($"Date: {billingDate}").FontSize(8)
                                        .FontColor("#64748b").AlignRight();
                                });
                            });
                        });

                        // ── STATUS BAR ───────────────────────────
                        col.Item().Background("#1e293b")
                            .PaddingHorizontal(28).PaddingVertical(8).Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"Job Card: #JC-{jId:D4}  |  Payment: {paymentType}")
                                .FontSize(8).FontColor("#64748b");
                            row.ConstantItem(100).AlignRight()
                                .Background(statusColor).PaddingHorizontal(10).PaddingVertical(4)
                                .AlignCenter()
                                .Text(status?.ToUpper() ?? "PENDING")
                                .Bold().FontSize(8).FontColor("#ffffff");
                        });

                        // ── BODY ─────────────────────────────────
                        col.Item().PaddingHorizontal(28).PaddingTop(20).Column(body =>
                        {
                            // ── ROW 1: Bill To + Vehicle Details ─
                            body.Item().PaddingBottom(16).Row(row =>
                            {
                                // Bill To (Owner)
                                row.RelativeItem().Border(1).BorderColor("#e2e8f0").Column(c =>
                                {
                                    c.Item().Background("#4338ca").Padding(8)
                                        .Text("BILL TO").Bold().FontSize(8).FontColor("#ffffff");
                                    c.Item().Padding(12).Column(inner =>
                                    {
                                        inner.Item().Text(ownerName).Bold().FontSize(13).FontColor("#0f172a");
                                        if (!string.IsNullOrEmpty(ownerEmail))
                                            inner.Item().PaddingTop(4).Text($"✉  {ownerEmail}")
                                                .FontSize(9).FontColor("#475569");
                                        if (!string.IsNullOrEmpty(ownerContact))
                                            inner.Item().PaddingTop(3).Text($"📞  {ownerContact}")
                                                .FontSize(9).FontColor("#475569");
                                    });
                                });

                                row.ConstantItem(14);

                                // Vehicle Details
                                row.RelativeItem().Border(1).BorderColor("#e2e8f0").Column(c =>
                                {
                                    c.Item().Background("#0f172a").Padding(8)
                                        .Text("VEHICLE DETAILS").Bold().FontSize(8).FontColor("#ffffff");
                                    c.Item().Padding(12).Column(inner =>
                                    {
                                        inner.Item().Text($"{carName} {carModel}").Bold().FontSize(12).FontColor("#0f172a");
                                        inner.Item().PaddingTop(4).Row(r =>
                                        {
                                            r.RelativeItem().Text("Number Plate").FontSize(8).FontColor("#64748b");
                                            r.RelativeItem().AlignRight()
                                                .Text(string.IsNullOrEmpty(numberPlate) ? "—" : numberPlate)
                                                .Bold().FontSize(9).FontColor("#4338ca");
                                        });
                                        inner.Item().PaddingTop(3).Row(r =>
                                        {
                                            r.RelativeItem().Text("Year").FontSize(8).FontColor("#64748b");
                                            r.RelativeItem().AlignRight()
                                                .Text(string.IsNullOrEmpty(carYear) ? "—" : carYear)
                                                .FontSize(9).FontColor("#0f172a");
                                        });
                                    });
                                });
                            });

                            // ── ROW 2: Service Info ───────────────
                            body.Item().PaddingBottom(16).Border(1).BorderColor("#e2e8f0").Column(c =>
                            {
                                c.Item().Background("#f8fafc").Padding(8)
                                    .Text("SERVICE INFORMATION").Bold().FontSize(8).FontColor("#4338ca");
                                c.Item().Padding(12).Row(row =>
                                {
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Problem / Issue").FontSize(8).FontColor("#64748b");
                                        inner.Item().PaddingTop(3)
                                            .Text(string.IsNullOrEmpty(problem) ? "—" : problem)
                                            .Bold().FontSize(10).FontColor("#0f172a");
                                    });
                                    row.ConstantItem(1).Background("#e2e8f0");
                                    row.ConstantItem(14);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Assigned Mechanic").FontSize(8).FontColor("#64748b");
                                        inner.Item().PaddingTop(3)
                                            .Text(string.IsNullOrEmpty(mechName) ? "—" : mechName)
                                            .Bold().FontSize(10).FontColor("#0f172a");
                                    });
                                    row.ConstantItem(1).Background("#e2e8f0");
                                    row.ConstantItem(14);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Date In").FontSize(8).FontColor("#64748b");
                                        inner.Item().PaddingTop(3)
                                            .Text(string.IsNullOrEmpty(dateIn) ? "—" : dateIn)
                                            .FontSize(9).FontColor("#0f172a");
                                    });
                                    row.ConstantItem(1).Background("#e2e8f0");
                                    row.ConstantItem(14);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Date Out").FontSize(8).FontColor("#64748b");
                                        inner.Item().PaddingTop(3)
                                            .Text(string.IsNullOrEmpty(dateOut) ? "—" : dateOut)
                                            .FontSize(9).FontColor("#0f172a");
                                    });
                                    row.ConstantItem(1).Background("#e2e8f0");
                                    row.ConstantItem(14);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("Job Status").FontSize(8).FontColor("#64748b");
                                        inner.Item().PaddingTop(3)
                                            .Text(string.IsNullOrEmpty(jobStatus) ? "—" : jobStatus)
                                            .Bold().FontSize(9)
                                            .FontColor(jobStatus == "Completed" || jobStatus == "Delivered" ? "#16a34a"
                                                     : jobStatus == "In Progress" || jobStatus == "Assigned" ? "#4338ca"
                                                     : jobStatus == "Testing" ? "#d97706" : "#64748b");
                                    });
                                });
                            });

                            // ── PARTS TABLE ───────────────────────
                            body.Item().PaddingBottom(4)
                                .Text("PARTS & SERVICES").Bold().FontSize(9).FontColor("#4338ca");

                            body.Item().Background("#0f172a").Padding(9).Row(row =>
                            {
                                row.ConstantItem(28).Text("#").Bold().FontSize(8).FontColor("#94a3b8");
                                row.RelativeItem().Text("Description").Bold().FontSize(9).FontColor("#ffffff");
                                row.ConstantItem(110).AlignRight()
                                    .Text("Amount (Rs.)").Bold().FontSize(9).FontColor("#ffffff");
                            });

                            if (parts.Any())
                            {
                                int idx = 1; bool alt = false;
                                foreach (var (name, price) in parts)
                                {
                                    body.Item().Background(alt ? "#f8fafc" : "#ffffff")
                                        .BorderBottom(1).BorderColor("#f1f5f9").Padding(9).Row(row =>
                                    {
                                        row.ConstantItem(28).Text(idx.ToString())
                                            .FontSize(8).FontColor("#94a3b8");
                                        row.RelativeItem().Text(name).FontSize(10).FontColor("#0f172a");
                                        row.ConstantItem(110).AlignRight()
                                            .Text($"Rs. {price:N0}").FontSize(10).FontColor("#0f172a");
                                    });
                                    idx++; alt = !alt;
                                }
                            }
                            else
                            {
                                body.Item().Background("#f8fafc").BorderBottom(1).BorderColor("#f1f5f9")
                                    .Padding(9).Row(row =>
                                {
                                    row.ConstantItem(28).Text("1").FontSize(8).FontColor("#94a3b8");
                                    row.RelativeItem().Text("Service Charges").FontSize(10).FontColor("#0f172a");
                                    row.ConstantItem(110).AlignRight()
                                        .Text($"Rs. {partCost ?? 0:N0}").FontSize(10).FontColor("#0f172a");
                                });
                            }

                            // ── TOTALS ────────────────────────────
                            body.Item().PaddingTop(14).AlignRight().Column(c =>
                            {
                                c.Item().BorderBottom(1).BorderColor("#e2e8f0").PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(180).Text("Subtotal (Parts / Labour)")
                                        .FontSize(9).FontColor("#64748b");
                                    r.ConstantItem(110).AlignRight()
                                        .Text($"Rs. {partCost ?? 0:N0}").FontSize(9);
                                });
                                c.Item().PaddingTop(5).BorderBottom(1).BorderColor("#e2e8f0")
                                    .PaddingBottom(5).Row(r =>
                                {
                                    r.ConstantItem(180).Text("GST").FontSize(9).FontColor("#64748b");
                                    r.ConstantItem(110).AlignRight()
                                        .Text($"Rs. {gst ?? 0:N0}").FontSize(9);
                                });
                                c.Item().PaddingTop(6).Background("#0f172a").Padding(11).Row(r =>
                                {
                                    r.ConstantItem(180).Text("TOTAL AMOUNT")
                                        .Bold().FontSize(12).FontColor("#ffffff");
                                    r.ConstantItem(110).AlignRight()
                                        .Text($"Rs. {totalCost ?? 0:N0}")
                                        .Bold().FontSize(13).FontColor("#818cf8");
                                });
                            });

                            // ── NOTE ──────────────────────────────
                            body.Item().PaddingTop(20).Background("#f8fafc")
                                .Border(1).BorderColor("#e2e8f0").Padding(10).Column(c =>
                            {
                                c.Item().Text("NOTE").Bold().FontSize(8).FontColor("#4338ca");
                                c.Item().PaddingTop(3)
                                    .Text("Payment is due upon receipt. Please retain this invoice for your records.")
                                    .FontSize(8).FontColor("#64748b");
                            });
                        });

                        // ── FOOTER ───────────────────────────────
                        col.Item().PaddingTop(16).Background("#0f172a")
                            .PaddingHorizontal(28).PaddingVertical(14).Row(row =>
                        {
                            row.RelativeItem()
                                .Text("VSMS — Vehicle Service Management System")
                                .FontSize(8).FontColor("#475569");
                            row.ConstantItem(220).AlignRight()
                                .Text("This is a computer-generated invoice.")
                                .FontSize(8).FontColor("#475569");
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
