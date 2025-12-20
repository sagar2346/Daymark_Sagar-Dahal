using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DailyJournalApp.Models;

namespace DailyJournalApp.Services
{
    public class ExportService
    {
        public async Task<string> ExportJournalToPdfAsync(List<JournalEntry> entries, string filePath)
        {
            // QuestPDF works by defining a Document
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                    page.Header().Text("Daily Journal Records")
                        .SemiBold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.DeepPurple.Medium);

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        foreach (var entry in entries)
                        {
                            column.Item().PaddingBottom(15).Column(entryColumn =>
                            {
                                entryColumn.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(entry.EntryDate.ToLongDateString()).Bold().FontSize(14);
                                    row.AutoItem().Text(entry.PrimaryMood ?? "").Italic();
                                });
                                
                                entryColumn.Item().Text(entry.Title).SemiBold();
                                entryColumn.Item().PaddingTop(5).Text(entry.Content);
                                entryColumn.Item().PaddingTop(5).LineHorizontal(1);
                            });
                        }
                    });

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
