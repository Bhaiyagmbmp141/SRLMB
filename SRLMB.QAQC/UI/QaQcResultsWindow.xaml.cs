using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using SRLMB.QAQC.Core;
using SRLMB.QAQC.Reporting;

namespace SRLMB.QAQC.UI
{
    public partial class QaQcResultsWindow : Window
    {
        private readonly Document _doc;
        private readonly UIDocument _uidoc;
        private readonly ObservableCollection<CheckListItem> _checkItems;
        private readonly ObservableCollection<IssueRow> _rows = new ObservableCollection<IssueRow>();
        private List<CheckResult> _lastResults = new List<CheckResult>();

        public QaQcResultsWindow(UIDocument uidoc)
        {
            InitializeComponent();

            _uidoc = uidoc;
            _doc = uidoc.Document;

            _checkItems = new ObservableCollection<CheckListItem>(
                CheckRunner.GetAllChecks().Select(c => new CheckListItem(c)));

            ChecksListBox.ItemsSource = _checkItems;
            ResultsGrid.ItemsSource = _rows;

            RunChecks();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckListItem item in _checkItems) item.IsSelected = true;
        }

        private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckListItem item in _checkItems) item.IsSelected = false;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e) => RunChecks();

        private void RunChecks()
        {
            var selectedChecks = _checkItems.Where(i => i.IsSelected).Select(i => i.Check).ToList();

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                _lastResults = CheckRunner.RunAll(_doc, selectedChecks);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            _rows.Clear();

            foreach (CheckResult result in _lastResults)
            {
                if (result.Error != null)
                {
                    _rows.Add(new IssueRow
                    {
                        Severity = "Error",
                        Category = result.Category,
                        Check = result.CheckName,
                        Element = "",
                        Message = $"Check failed to run: {result.Error}",
                        Recommendation = "",
                        ElementId = ElementId.InvalidElementId
                    });
                    continue;
                }

                foreach (CheckIssue issue in result.Issues)
                {
                    _rows.Add(new IssueRow
                    {
                        Severity = issue.Severity.ToString(),
                        Category = result.Category,
                        Check = result.CheckName,
                        Element = issue.ElementDescription,
                        Message = issue.Message,
                        Recommendation = issue.Recommendation,
                        ElementId = issue.ElementId
                    });
                }
            }

            ApplySeverityFilter();
            UpdateSummary();
        }

        private void SeverityFilterChanged(object sender, RoutedEventArgs e) => ApplySeverityFilter();

        private void ApplySeverityFilter()
        {
            ICollectionView? view = CollectionViewSource.GetDefaultView(ResultsGrid.ItemsSource);
            if (view == null) return;

            bool showErrors = ShowErrors.IsChecked == true;
            bool showWarnings = ShowWarnings.IsChecked == true;
            bool showInfo = ShowInfo.IsChecked == true;

            view.Filter = obj =>
            {
                var row = (IssueRow)obj;
                return row.Severity switch
                {
                    "Error" => showErrors,
                    "Warning" => showWarnings,
                    "Info" => showInfo,
                    _ => true
                };
            };
        }

        private void UpdateSummary()
        {
            int errors = _rows.Count(r => r.Severity == "Error");
            int warnings = _rows.Count(r => r.Severity == "Warning");
            int info = _rows.Count(r => r.Severity == "Info");

            SummaryText.Text =
                $"{errors} error(s), {warnings} warning(s), {info} info — across {_lastResults.Count} check(s).";
        }

        private void ResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ResultsGrid.SelectedItem is not IssueRow row) return;
            if (row.ElementId == null || row.ElementId == ElementId.InvalidElementId) return;

            try
            {
                _uidoc.Selection.SetElementIds(new List<ElementId> { row.ElementId });
                _uidoc.ShowElements(row.ElementId);
            }
            catch (Exception)
            {
                // Best-effort convenience action: the element may not be visible/
                // selectable from the active view (e.g. it lives on a sheet/view
                // that isn't open). Failing silently is preferable to blocking the
                // QA/QC review with a dialog over a non-critical action.
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "HTML report (*.html)|*.html",
                FileName = $"SRLMB_QAQC_Report_{DateTime.Now:yyyyMMdd_HHmm}.html"
            };

            if (dialog.ShowDialog() == true)
            {
                string html = HtmlReportWriter.Build(_doc.Title, _lastResults);
                File.WriteAllText(dialog.FileName, html);
                TaskDialog.Show("SRLMB – QA/QC", $"Report exported to:\n{dialog.FileName}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
