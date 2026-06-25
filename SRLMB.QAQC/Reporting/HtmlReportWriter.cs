using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.Reporting
{
    public static class HtmlReportWriter
    {
        public static string Build(string modelTitle, IReadOnlyList<CheckResult> results)
        {
            int errors = results.Sum(r => r.Issues.Count(i => i.Severity == CheckSeverity.Error));
            int warnings = results.Sum(r => r.Issues.Count(i => i.Severity == CheckSeverity.Warning));
            int infos = results.Sum(r => r.Issues.Count(i => i.Severity == CheckSeverity.Info));

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/>");
            sb.AppendLine($"<title>SRLMB Model QA/QC Report — {Escape(modelTitle)}</title>");
            sb.AppendLine(@"<style>
body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#222;}
h1{margin-bottom:0;} .sub{color:#666;margin-top:4px;}
table{border-collapse:collapse;width:100%;margin-top:10px;}
th,td{border:1px solid #ddd;padding:6px 8px;font-size:13px;text-align:left;vertical-align:top;}
th{background:#f4f4f4;}
tr.Error{background:#fadada;} tr.Warning{background:#fff3cd;} tr.Info{background:#e7f1ff;}
.summary span{display:inline-block;margin-right:18px;font-weight:bold;}
.category{margin-top:28px;}
</style></head><body>");

            sb.AppendLine("<h1>SRLMB Model QA/QC Report</h1>");
            sb.AppendLine($"<div class=\"sub\">Model: {Escape(modelTitle)} &nbsp;|&nbsp; Generated: {DateTime.Now:yyyy-MM-dd HH:mm}</div>");
            sb.AppendLine(
                "<div class=\"summary\">" +
                $"<span style=\"color:#a00\">{errors} Errors</span>" +
                $"<span style=\"color:#a60\">{warnings} Warnings</span>" +
                $"<span style=\"color:#069\">{infos} Info</span>" +
                "</div>");

            foreach (var category in results.GroupBy(r => r.Category))
            {
                sb.AppendLine($"<div class=\"category\"><h2>{Escape(category.Key)}</h2>");

                foreach (CheckResult result in category)
                {
                    sb.AppendLine($"<h3>{Escape(result.CheckName)}</h3>");

                    if (result.Error != null)
                    {
                        sb.AppendLine($"<p style=\"color:#a00\">Check failed to run: {Escape(result.Error)}</p>");
                        continue;
                    }

                    if (result.Issues.Count == 0)
                    {
                        sb.AppendLine("<p style=\"color:#2a7\">No issues found.</p>");
                        continue;
                    }

                    sb.AppendLine("<table><tr><th>Severity</th><th>Element</th><th>Message</th><th>Recommendation</th></tr>");

                    foreach (CheckIssue issue in result.Issues)
                    {
                        sb.AppendLine(
                            $"<tr class=\"{issue.Severity}\"><td>{issue.Severity}</td>" +
                            $"<td>{Escape(issue.ElementDescription)}</td>" +
                            $"<td>{Escape(issue.Message)}</td>" +
                            $"<td>{Escape(issue.Recommendation)}</td></tr>");
                    }

                    sb.AppendLine("</table>");
                }

                sb.AppendLine("</div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string Escape(string text) => WebUtility.HtmlEncode(text ?? string.Empty);
    }
}
