using System.Text;

namespace ShiftPlanner.Application.Imports;

/// <summary>
/// The CSV layout shared by the shift import and the weekly schedule export, so an exported week
/// can be edited and imported again.
/// </summary>
public static class ShiftCsvFormat
{
    public const string DateColumn = "date";
    public const string ShiftTypeColumn = "shiftType";
    public const string RequiredSkillColumn = "requiredSkill";
    public const string EmployeeColumn = "employee";

    public const string DateFormat = "yyyy-MM-dd";

    public static readonly string[] Columns =
    [
        DateColumn,
        ShiftTypeColumn,
        RequiredSkillColumn,
        EmployeeColumn
    ];

    /// <summary>
    /// Spreadsheet programs run cells that start with these characters as formulas
    /// (CSV injection), so imported values may not start with them.
    /// </summary>
    private static readonly char[] FormulaPrefixes = ['=', '+', '-', '@', '\t', '\r'];

    public static bool StartsLikeFormula(string value)
    {
        return value.Length > 0 && FormulaPrefixes.Contains(value[0]);
    }

    public static string FormatField(string value)
    {
        // An apostrophe makes spreadsheet programs show the value as text instead of running it.
        var safeValue = StartsLikeFormula(value) ? "'" + value : value;

        var needsQuotes = safeValue.IndexOfAny([',', ';', '"', '\n', '\r']) >= 0;

        return needsQuotes
            ? "\"" + safeValue.Replace("\"", "\"\"") + "\""
            : safeValue;
    }

    public static string FormatLine(IEnumerable<string> fields)
    {
        return string.Join(",", fields.Select(FormatField));
    }

    /// <summary>
    /// Splits CSV text into records following RFC 4180: quoted fields may contain the delimiter,
    /// line breaks and doubled quotes. Each record keeps the 1-based line number it started on.
    /// </summary>
    public static List<(int LineNumber, List<string> Fields)> ReadRecords(
        string content,
        char delimiter,
        out bool hasUnterminatedQuote)
    {
        var records = new List<(int, List<string>)>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var lineNumber = 1;
        var recordStartLine = 1;

        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];

            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < content.Length && content[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    if (character == '\n')
                    {
                        lineNumber++;
                    }

                    field.Append(character);
                }

                continue;
            }

            if (character == '"' && field.Length == 0)
            {
                inQuotes = true;
            }
            else if (character == delimiter)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (character == '\r' || character == '\n')
            {
                if (character == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                {
                    index++;
                }

                fields.Add(field.ToString());
                field.Clear();
                AddRecord(records, recordStartLine, fields);
                fields = [];
                lineNumber++;
                recordStartLine = lineNumber;
            }
            else
            {
                field.Append(character);
            }
        }

        hasUnterminatedQuote = inQuotes;

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            AddRecord(records, recordStartLine, fields);
        }

        return records;
    }

    private static void AddRecord(
        List<(int, List<string>)> records,
        int lineNumber,
        List<string> fields)
    {
        // Blank lines carry no data and are skipped.
        if (fields.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        records.Add((lineNumber, fields));
    }
}
