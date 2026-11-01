namespace ShiftPlanner.Application.Imports;

public class ShiftCsvRow
{
    /// <summary>The line in the file where the row starts (the header is line 1).</summary>
    public int RowNumber { get; set; }

    public string Date { get; set; } = string.Empty;

    public string ShiftType { get; set; } = string.Empty;

    public string RequiredSkill { get; set; } = string.Empty;

    public string Employee { get; set; } = string.Empty;

    public int FieldCount { get; set; }
}

public class ShiftCsvParseResult
{
    public List<string> FileErrors { get; set; } = [];

    public int ExpectedFieldCount { get; set; }

    public List<ShiftCsvRow> Rows { get; set; } = [];
}

/// <summary>
/// Reads the raw CSV into rows of trimmed text. It only checks the file's structure;
/// <see cref="ShiftImportValidator"/> decides whether each row is a valid shift.
/// </summary>
public static class ShiftCsvParser
{
    public const int MaxRows = 1000;

    public static ShiftCsvParseResult Parse(string content)
    {
        var result = new ShiftCsvParseResult();

        // Spreadsheet programs often save UTF-8 with a byte order mark.
        content = content.TrimStart('﻿');

        if (string.IsNullOrWhiteSpace(content))
        {
            result.FileErrors.Add("The file is empty.");
            return result;
        }

        var delimiter = DetectDelimiter(content);
        var records = ShiftCsvFormat.ReadRecords(content, delimiter, out var hasUnterminatedQuote);

        if (hasUnterminatedQuote)
        {
            result.FileErrors.Add("The file ends inside a quoted value; a closing quote (\") is missing.");
            return result;
        }

        var (_, header) = records[0];
        var columnIndexes = ReadHeader(header, result.FileErrors);

        if (result.FileErrors.Count > 0)
        {
            return result;
        }

        var dataRecords = records.Skip(1).ToList();

        if (dataRecords.Count == 0)
        {
            result.FileErrors.Add("The file has a header but no shift rows.");
            return result;
        }

        if (dataRecords.Count > MaxRows)
        {
            result.FileErrors.Add($"The file has {dataRecords.Count} rows; at most {MaxRows} can be imported at once.");
            return result;
        }

        result.ExpectedFieldCount = header.Count;

        foreach (var (lineNumber, fields) in dataRecords)
        {
            result.Rows.Add(new ShiftCsvRow
            {
                RowNumber = lineNumber,
                Date = FieldOrEmpty(fields, columnIndexes, ShiftCsvFormat.DateColumn),
                ShiftType = FieldOrEmpty(fields, columnIndexes, ShiftCsvFormat.ShiftTypeColumn),
                RequiredSkill = FieldOrEmpty(fields, columnIndexes, ShiftCsvFormat.RequiredSkillColumn),
                Employee = FieldOrEmpty(fields, columnIndexes, ShiftCsvFormat.EmployeeColumn),
                FieldCount = fields.Count
            });
        }

        return result;
    }

    private static char DetectDelimiter(string content)
    {
        // Danish spreadsheet programs save CSV with semicolons; the header decides.
        var lineEnd = content.IndexOfAny(['\r', '\n']);
        var headerLine = lineEnd < 0 ? content : content[..lineEnd];

        return headerLine.Count(character => character == ';') >
               headerLine.Count(character => character == ',')
            ? ';'
            : ',';
    }

    private static Dictionary<string, int> ReadHeader(List<string> header, List<string> fileErrors)
    {
        var indexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < header.Count; index++)
        {
            var name = header[index].Trim();
            var column = ShiftCsvFormat.Columns.FirstOrDefault(column =>
                string.Equals(column, name, StringComparison.OrdinalIgnoreCase));

            if (column is null)
            {
                fileErrors.Add(
                    $"Unknown column '{name}'. Expected the columns {string.Join(", ", ShiftCsvFormat.Columns)}.");
                continue;
            }

            if (!indexes.TryAdd(column, index))
            {
                fileErrors.Add($"The column '{column}' appears more than once.");
            }
        }

        foreach (var required in new[]
                 {
                     ShiftCsvFormat.DateColumn,
                     ShiftCsvFormat.ShiftTypeColumn,
                     ShiftCsvFormat.RequiredSkillColumn
                 })
        {
            if (!indexes.ContainsKey(required))
            {
                fileErrors.Add($"The required column '{required}' is missing.");
            }
        }

        return indexes;
    }

    private static string FieldOrEmpty(
        List<string> fields,
        Dictionary<string, int> columnIndexes,
        string column)
    {
        return columnIndexes.TryGetValue(column, out var index) && index < fields.Count
            ? fields[index].Trim()
            : string.Empty;
    }
}
