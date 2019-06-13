using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace _011EleFilter {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_StackedWalls);
            collector.WherePasses(filter).OfClass(typeof(Wall));
            ICollection<Element> found = collector.ToElements();
            foreach (Element ele in found) {
                TaskDialog.Show("T", ele.Name);
            }

            FilteredElementCollector collectorLev = new FilteredElementCollector(doc);
            //ElementFilter levFilter=new ElementClassFilter(typeof(Level));
            collectorLev = collectorLev.OfCategory(BuiltInCategory.OST_Levels);
            Level levs1 = collectorLev.First(m => m.Name == "标高 1") as Level;//通过名称引用
            //Level levs1 = collectorLev.First(m => ((Level)m).Elevation == 0) as Level;//通过名称引用

            #region 通过高度值引用

            //Level idd = null;
            //foreach (Element e in collectorLev) {
            //    if (e is Level) {
            //        Level ee = e as Level;
            //        if (ee.Elevation.Equals(8200 / 304.8)) {
            //            idd = e;
            //        }
            //    }
            //}

            #endregion

            if (levs1 != null) {
                TaskDialog.Show("Lev", levs1.Name.ToString() + "\n\t" + (levs1.Id.IntegerValue).ToString());
            }
            return Result.Succeeded;
        }
    }
}