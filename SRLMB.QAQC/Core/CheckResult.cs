using System;
using System.Collections.Generic;

namespace SRLMB.QAQC.Core
{
    public sealed class CheckResult
    {
        public string CheckName { get; }
        public string Category { get; }
        public IReadOnlyList<CheckIssue> Issues { get; }
        public TimeSpan Elapsed { get; }
        public string? Error { get; }

        public CheckResult(
            string checkName,
            string category,
            IReadOnlyList<CheckIssue> issues,
            TimeSpan elapsed,
            string? error = null)
        {
            CheckName = checkName;
            Category = category;
            Issues = issues;
            Elapsed = elapsed;
            Error = error;
        }

        public static CheckResult Failed(string checkName, string category, string error) =>
            new CheckResult(checkName, category, Array.Empty<CheckIssue>(), TimeSpan.Zero, error);
    }
}
