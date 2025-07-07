using System;

namespace GitDWG.Services
{
    public class CommitInfo
    {
        public string Sha { get; set; } = string.Empty;
        public string ShortSha { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset CommitDate { get; set; }
    }
}