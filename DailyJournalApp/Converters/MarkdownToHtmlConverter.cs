using System.Globalization;
using Markdig;

namespace DailyJournalApp.Converters
{
    public class MarkdownToHtmlConverter : IValueConverter
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string markdownText)
            {
                if (string.IsNullOrWhiteSpace(markdownText))
                    return string.Empty;

                return Markdown.ToHtml(markdownText, Pipeline);
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
