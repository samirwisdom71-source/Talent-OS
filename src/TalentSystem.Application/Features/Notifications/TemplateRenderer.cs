namespace TalentSystem.Application.Features.Notifications;

internal static class TemplateRenderer
{
    public static string Apply(string template, IReadOnlyDictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        var result = template;
        foreach (var kv in tokens)
        {
            result = result.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
        }

        return result;
    }
}
