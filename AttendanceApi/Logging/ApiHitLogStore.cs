using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

/// <summary>Shared log persistence used by request middleware and the diagnostic read endpoint.</summary>
public sealed class ApiHitLogStore : IDisposable
{
    private readonly string _directory;
    private readonly SemaphoreSlim _logLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly UTF8Encoding _encoding = new(false);

    public ApiHitLogStore(IHostEnvironment environment)
    {
        _directory = Path.Combine(environment.ContentRootPath, "App_Data", "api-hits");
    }

    public async Task AppendAsync(AttendanceApiHitLog hit)
    {
        await _logLock.WaitAsync();
        try
        {
            Directory.CreateDirectory(_directory);
            var logFile = GetLogFile(hit.ReceivedAt.UtcDateTime);
            await File.AppendAllTextAsync(logFile,
                JsonSerializer.Serialize(hit, _jsonOptions) + Environment.NewLine,
                _encoding);
        }
        finally
        {
            _logLock.Release();
        }
    }

    public async Task<AttendanceApiHitLog[]> ReadHitsAsync(
        DateTime selectedDate, int limit, int? statusCode)
    {
        var logFile = GetLogFile(selectedDate);
        var hits = new Queue<AttendanceApiHitLog>();

        await _logLock.WaitAsync();
        try
        {
            if (File.Exists(logFile))
            {
                // Keep only the requested last hits in memory while reading the daily file.
                using var reader = new StreamReader(logFile, Encoding.UTF8);
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    AttendanceApiHitLog hit;
                    try
                    {
                        hit = JsonSerializer.Deserialize<AttendanceApiHitLog>(line, _jsonOptions);
                    }
                    catch (JsonException)
                    {
                        // Ignore incomplete JSON left by an interrupted file write.
                        continue;
                    }

                    if (hit == null || (statusCode.HasValue && hit.HttpStatusCode != statusCode.Value))
                        continue;

                    hits.Enqueue(hit);
                    if (hits.Count > limit)
                        hits.Dequeue();
                }
            }
        }
        finally
        {
            _logLock.Release();
        }

        return hits.Reverse().ToArray();
    }

    private string GetLogFile(DateTime date) =>
        Path.Combine(_directory,
            "api-hits-" + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".jsonl");

    public void Dispose() => _logLock.Dispose();
}
