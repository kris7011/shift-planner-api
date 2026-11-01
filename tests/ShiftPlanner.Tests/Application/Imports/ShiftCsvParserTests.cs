using ShiftPlanner.Application.Imports;

namespace ShiftPlanner.Tests.Application.Imports;

public class ShiftCsvParserTests
{
    [Fact]
    public void Parse_ReadsRowsWithTheirLineNumbers()
    {
        var result = ShiftCsvParser.Parse(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Day,CT,Kris\n" +
            "\n" +
            "2026-05-18,Night,MRI,\n");

        Assert.Empty(result.FileErrors);
        Assert.Equal(2, result.Rows.Count);

        Assert.Equal(2, result.Rows[0].RowNumber);
        Assert.Equal("2026-05-18", result.Rows[0].Date);
        Assert.Equal("Day", result.Rows[0].ShiftType);
        Assert.Equal("CT", result.Rows[0].RequiredSkill);
        Assert.Equal("Kris", result.Rows[0].Employee);

        // The blank line is skipped but still counted, so row numbers match the file.
        Assert.Equal(4, result.Rows[1].RowNumber);
        Assert.Equal(string.Empty, result.Rows[1].Employee);
    }

    [Fact]
    public void Parse_AcceptsReorderedCaseInsensitiveColumnsAndSemicolons()
    {
        var result = ShiftCsvParser.Parse(
            "﻿EMPLOYEE;RequiredSkill;ShiftType;Date\r\n" +
            "Mette;MRI;Evening;2026-05-19\r\n");

        Assert.Empty(result.FileErrors);
        var row = Assert.Single(result.Rows);
        Assert.Equal("2026-05-19", row.Date);
        Assert.Equal("Evening", row.ShiftType);
        Assert.Equal("MRI", row.RequiredSkill);
        Assert.Equal("Mette", row.Employee);
    }

    [Fact]
    public void Parse_EmployeeColumnIsOptional()
    {
        var result = ShiftCsvParser.Parse(
            "date,shiftType,requiredSkill\n" +
            "2026-05-18,Day,CT\n");

        Assert.Empty(result.FileErrors);
        Assert.Equal(string.Empty, Assert.Single(result.Rows).Employee);
    }

    [Fact]
    public void Parse_HandlesQuotedValues()
    {
        var result = ShiftCsvParser.Parse(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Day,\"CT, contrast\",\"Jensen, \"\"Kris\"\"\"\n");

        var row = Assert.Single(result.Rows);
        Assert.Equal("CT, contrast", row.RequiredSkill);
        Assert.Equal("Jensen, \"Kris\"", row.Employee);
        Assert.Equal(4, row.FieldCount);
    }

    [Fact]
    public void Parse_RecordsTheFieldCountOfShortRows()
    {
        var result = ShiftCsvParser.Parse(
            "date,shiftType,requiredSkill,employee\n" +
            "2026-05-18,Day\n");

        Assert.Equal(4, result.ExpectedFieldCount);
        Assert.Equal(2, Assert.Single(result.Rows).FieldCount);
    }

    [Theory]
    [InlineData("", "The file is empty.")]
    [InlineData("date,shiftType,requiredSkill,employee\n", "The file has a header but no shift rows.")]
    [InlineData("date,shift,requiredSkill\n2026-05-18,Day,CT\n", "Unknown column 'shift'.")]
    [InlineData("date,requiredSkill\n2026-05-18,CT\n", "The required column 'shiftType' is missing.")]
    [InlineData("date,date,shiftType,requiredSkill\n", "The column 'date' appears more than once.")]
    [InlineData("date,shiftType,requiredSkill\n2026-05-18,Day,\"CT\n", "closing quote")]
    public void Parse_ReportsFileErrors(string content, string expectedError)
    {
        var result = ShiftCsvParser.Parse(content);

        Assert.Contains(result.FileErrors, error => error.Contains(expectedError));
        Assert.Empty(result.Rows);
    }

    [Fact]
    public void Parse_RefusesMoreThanTheMaximumNumberOfRows()
    {
        var rows = string.Concat(Enumerable.Repeat("2026-05-18,Day,CT,\n", ShiftCsvParser.MaxRows + 1));

        var result = ShiftCsvParser.Parse("date,shiftType,requiredSkill,employee\n" + rows);

        Assert.Contains(result.FileErrors, error => error.Contains($"at most {ShiftCsvParser.MaxRows}"));
    }
}
