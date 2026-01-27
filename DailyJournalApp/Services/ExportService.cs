using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    /// <summary>
    /// Specialized service for generating high-quality documentation from journal data.
    /// Utilizes QuestPDF for sophisticated layout design and PDF generation.
    /// </summary>
    public class ExportService
    {
        /// <summary>
        /// Orchestrates the data-to-PDF transformation process.
        /// Maps journal entry properties into a structured A4 document layout.
        /// </summary>
        /// <param name="entries">The list of journal entries to include in the report.</param>
        /// <param name="filePath">The target local path where the PDF will be saved.</param>
        /// <returns>The confirmed file path of the generated PDF.</returns>
        public async Task<string> ExportJournalToPdfAsync(List<JournalEntry> entries, string filePath)
        {
            // QuestPDF uses a fluent API to define the document structure
            Document.Create(container =>
            {
                // Define the global page settings
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                    // --- Header Section ---
                    page.Header().Text("Daily Journal Records")
                        .SemiBold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.DeepPurple.Medium);

                    // --- Content Section (The main body of the journal) ---
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        foreach (var entry in entries)
                        {
                            // Each entry item container
                            column.Item().PaddingBottom(15).Column(entryColumn =>
                            {
                                // Entry Metadata Row (Date & Mood)
                                entryColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(entry.EntryDate.ToLongDateString()).Bold().FontSize(14);
                                    row.AutoItem().Text(entry.PrimaryMood ?? "").Italic();
                                });
                                
                                // Title and Content
                                entryColumn.Item().Text(entry.Title).SemiBold();
                                entryColumn.Item().PaddingTop(5).Text(entry.Content);
                                
                                // Separation Line for readability
                                entryColumn.Item().PaddingTop(5).LineHorizontal(1);
                            });
                        }
                    });

                    // --- Footer Section (Page numbering) ---
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(filePath);

            return filePath;
        }
    }
}
