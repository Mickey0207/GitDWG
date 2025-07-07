using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GitDWG.Services
{
    public class PerformanceMonitorService : IDisposable
    {
        private readonly Timer _memoryMonitor;
        private readonly List<PerformanceMetric> _metrics;
        private readonly object _metricsLock = new();
        private bool _disposed;

        public PerformanceMonitorService()
        {
            _metrics = new List<PerformanceMetric>();
            
            // 每30秒監控一次記憶體使用情況
            _memoryMonitor = new Timer(MonitorMemory, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public void RecordOperation(string operationName, TimeSpan duration, bool success = true)
        {
            lock (_metricsLock)
            {
                _metrics.Add(new PerformanceMetric
                {
                    OperationName = operationName,
                    Duration = duration,
                    Timestamp = DateTime.Now,
                    Success = success,
                    MemoryUsed = GC.GetTotalMemory(false)
                });

                // 保持最近1000個記錄
                if (_metrics.Count > 1000)
                {
                    _metrics.RemoveRange(0, 100);
                }
            }
        }

        public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var success = true;
            
            try
            {
                var result = await operation().ConfigureAwait(false);
                return result;
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                RecordOperation(operationName, stopwatch.Elapsed, success);
            }
        }

        public T Measure<T>(string operationName, Func<T> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var success = true;
            
            try
            {
                var result = operation();
                return result;
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                RecordOperation(operationName, stopwatch.Elapsed, success);
            }
        }

        public void Measure(string operationName, Action operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var success = true;
            
            try
            {
                operation();
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                RecordOperation(operationName, stopwatch.Elapsed, success);
            }
        }

        private void MonitorMemory(object? state)
        {
            try
            {
                var memoryBefore = GC.GetTotalMemory(false);
                var memoryAfterGC = GC.GetTotalMemory(true);
                
                Debug.WriteLine($"Memory Monitor - Before GC: {FormatBytes(memoryBefore)}, After GC: {FormatBytes(memoryAfterGC)}");
                
                RecordOperation("MemoryMonitor", TimeSpan.Zero, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Memory monitoring error: {ex.Message}");
            }
        }

        public PerformanceReport GetReport(TimeSpan? period = null)
        {
            lock (_metricsLock)
            {
                var cutoffTime = DateTime.Now - (period ?? TimeSpan.FromHours(1));
                var recentMetrics = _metrics.Where(m => m.Timestamp >= cutoffTime).ToList();

                if (!recentMetrics.Any())
                {
                    return new PerformanceReport();
                }

                var groupedByOperation = recentMetrics.GroupBy(m => m.OperationName);
                var operationStats = new List<OperationStats>();

                foreach (var group in groupedByOperation)
                {
                    var durations = group.Where(g => g.Duration > TimeSpan.Zero).Select(g => g.Duration.TotalMilliseconds).ToList();
                    
                    if (durations.Any())
                    {
                        operationStats.Add(new OperationStats
                        {
                            OperationName = group.Key,
                            Count = group.Count(),
                            SuccessCount = group.Count(g => g.Success),
                            FailureCount = group.Count(g => !g.Success),
                            AverageDuration = TimeSpan.FromMilliseconds(durations.Average()),
                            MinDuration = TimeSpan.FromMilliseconds(durations.Min()),
                            MaxDuration = TimeSpan.FromMilliseconds(durations.Max()),
                            MedianDuration = TimeSpan.FromMilliseconds(GetMedian(durations))
                        });
                    }
                }

                return new PerformanceReport
                {
                    Period = period ?? TimeSpan.FromHours(1),
                    TotalOperations = recentMetrics.Count,
                    SuccessfulOperations = recentMetrics.Count(m => m.Success),
                    FailedOperations = recentMetrics.Count(m => !m.Success),
                    CurrentMemoryUsage = GC.GetTotalMemory(false),
                    OperationStats = operationStats,
                    ReportTime = DateTime.Now
                };
            }
        }

        private double GetMedian(List<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            var count = sorted.Count;
            
            if (count == 0) return 0;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            else
                return sorted[count / 2];
        }

        public string GetQuickStats()
        {
            var report = GetReport(TimeSpan.FromMinutes(5));
            
            return $"最近5分鐘: {report.TotalOperations}個操作, " +
                   $"成功率: {(report.TotalOperations > 0 ? (double)report.SuccessfulOperations / report.TotalOperations * 100 : 0):F1}%, " +
                   $"記憶體: {FormatBytes(report.CurrentMemoryUsage)}";
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            _memoryMonitor?.Dispose();
            _disposed = true;
            
            GC.SuppressFinalize(this);
        }
    }

    public class PerformanceMetric
    {
        public string OperationName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public long MemoryUsed { get; set; }
    }

    public class OperationStats
    {
        public string OperationName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public TimeSpan MedianDuration { get; set; }
        
        public double SuccessRate => Count > 0 ? (double)SuccessCount / Count * 100 : 0;
    }

    public class PerformanceReport
    {
        public TimeSpan Period { get; set; }
        public int TotalOperations { get; set; }
        public int SuccessfulOperations { get; set; }
        public int FailedOperations { get; set; }
        public long CurrentMemoryUsage { get; set; }
        public List<OperationStats> OperationStats { get; set; } = new();
        public DateTime ReportTime { get; set; }
        
        public double OverallSuccessRate => TotalOperations > 0 ? (double)SuccessfulOperations / TotalOperations * 100 : 0;
    }
}