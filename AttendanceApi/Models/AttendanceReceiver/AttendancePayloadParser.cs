using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Reads only known attendance fields. Unrecognized JSON properties are ignored.
/// Supports one JSON object, an array, or an object containing an attendanceRecords/punches/records array.
/// </summary>
public static class AttendancePayloadParser
{
    public const int MaxBatchSize = 500;

    public static bool TryGetRecords(
        JsonElement payload,
        out List<JsonElement> records,
        out bool single,
        out string error)
    {
        records = new List<JsonElement>();
        single = false;
        error = string.Empty;

        if (payload.ValueKind == JsonValueKind.Object)
        {
            if (TryGetProperty(payload, out var bulk, "attendanceRecords", "punches", "records"))
            {
                if (bulk.ValueKind != JsonValueKind.Array)
                {
                    error = "attendanceRecords/punches/records must be a JSON array.";
                    return false;
                }
                foreach (var record in bulk.EnumerateArray()) records.Add(record);
            }
            else
            {
                single = true;
                records.Add(payload);
            }
        }
        else if (payload.ValueKind == JsonValueKind.Array)
        {
            foreach (var record in payload.EnumerateArray()) records.Add(record);
        }
        else
        {
            error = "Send a JSON attendance object, array, or an object containing attendanceRecords.";
            return false;
        }

        if (records.Count == 0 || records.Count > MaxBatchSize)
        {
            error = $"Send between 1 and {MaxBatchSize} attendance records.";
            return false;
        }

        return true;
    }

    public static bool TryParse(
        JsonElement record,
        out IncomingAttendancePunch? punch,
        out string error)
    {
        punch = null;
        error = string.Empty;

        if (record.ValueKind != JsonValueKind.Object)
        {
            error = "Each attendance record must be a JSON object.";
            return false;
        }

        if (!RequiredText(record, 50, out var schoolCode, out error, "schoolCode", "school_code"))
            return false;
        if (!RequiredText(record, 200, out var studentName, out error, "studentName", "student_name"))
            return false;
        if (!RequiredText(record, 100, out var className, out error, "class", "className", "grade"))
            return false;
        if (!RequiredText(record, 100, out var classroom, out error, "classroom", "classRoom", "section"))
            return false;
        if (!RequiredText(record, 50, out var punchStatus, out error, "punchStatus", "status", "movement"))
            return false;
        if (!RequiredText(record, 80, out var punchTimeText, out error, "punchTime", "timestamp", "punchedAt"))
            return false;

        // Reject timestamps without an explicit offset: server-local interpretation is unsafe.
        var offsetProvided = Regex.IsMatch(
            punchTimeText!, @"(?:Z|[+-][0-9]{2}:[0-9]{2})$", RegexOptions.IgnoreCase);
        if (!offsetProvided ||
            !DateTimeOffset.TryParse(punchTimeText, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var punchTime))
        {
            error = "punchTime must be an ISO-8601 date/time with Z or a timezone offset.";
            return false;
        }

        punch = new IncomingAttendancePunch(
            schoolCode!, studentName!, className!, classroom!, punchTime, punchStatus!);
        return true;
    }

    private static bool RequiredText(JsonElement record, int maxLength,
        out string? value, out string error, params string[] keys)
    {
        value = null;
        error = string.Empty;
        if (!TryGetProperty(record, out var found, keys))
        {
            error = $"{keys[0]} is required.";
            return false;
        }

        if (found.ValueKind == JsonValueKind.String)
            value = found.GetString()?.Trim();
        else if (found.ValueKind == JsonValueKind.Number)
            value = found.GetRawText();

        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"{keys[0]} must be a non-empty string or number.";
            return false;
        }

        if (value.Length > maxLength)
        {
            error = $"{keys[0]} must not exceed {maxLength} characters.";
            return false;
        }

        return true;
    }

    private static bool TryGetProperty(JsonElement obj,
        out JsonElement result, params string[] names)
    {
        foreach (var property in obj.EnumerateObject())
        {
            foreach (var name in names)
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result = property.Value;
                    return true;
                }
            }
        }
        result = default;
        return false;
    }
}

public sealed record IncomingAttendancePunch(
    string SchoolCode,
    string StudentName,
    string ClassName,
    string Classroom,
    DateTimeOffset PunchTime,
    string PunchStatus);
