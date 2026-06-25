using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using SRLMB.QAQC.Checks;

namespace SRLMB.QAQC.Core
{
    public static class CheckRunner
    {
        public static IReadOnlyList<IQaQcCheck> GetAllChecks() => new IQaQcCheck[]
        {
            new ModelWarningsCheck(),
            new UnplacedRoomsCheck(),
            new UnenclosedRoomsCheck(),
            new RoomNamingCheck(),
            new ViewsNotOnSheetCheck(),
            new ViewsMissingTemplateCheck(),
            new SheetNamingCheck(),
            new ImportedCadCheck(),
            new InPlaceFamilyCheck(),
            new UnresolvedRevitLinkCheck(),
            new DefaultWorksetCheck(),
            new DimensionOverrideCheck()
        };

        public static List<CheckResult> RunAll(Document doc, IEnumerable<IQaQcCheck> checks)
        {
            var results = new List<CheckResult>();

            foreach (IQaQcCheck check in checks)
            {
                try
                {
                    results.Add(check.Run(doc));
                }
                catch (Exception ex)
                {
                    results.Add(CheckResult.Failed(check.Name, check.Category, ex.Message));
                }
            }

            return results;
        }
    }
}
