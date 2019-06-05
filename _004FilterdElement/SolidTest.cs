using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace _004FilterdElement {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            ICollection<Element> doors = CreateLogicAndFilter(doc);
            if (doors.Count==0)
            {
                TaskDialog.Show("no", "获取失败");
            }
            else
            {
                string s = "选取的对象ID是：";
                foreach (Element door in doors)
                {
                    s += "\n\t" + door.Id.IntegerValue;
                }

                TaskDialog.Show("Yes", s);
            }

            return Result.Succeeded;
        }

        public ICollection<Element> CreateLogicAndFilter(Document doc) {
            //通过查找同时属于门类别和族实例的所有元素来查找项目中的所有门实例。
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));//实例过滤器
            ElementClassFilter familySymbolFilter = new ElementClassFilter(typeof(FamilySymbol));//类型
            //为门创建类别过滤器
            ElementCategoryFilter doorsCategoryFilter =new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            //ElementCategoryFilter railingCategoryFilter =new ElementCategoryFilter(BuiltInCategory.OST_RailingSystem);//栏杆族获取失败

            //为所有门族实例创建逻辑和筛选器
            LogicalAndFilter doorInstanceFilter =new LogicalAndFilter(familyInstanceFilter, doorsCategoryFilter);
            //对活动文档中的元素应用筛选器
            FilteredElementCollector collector =new FilteredElementCollector(doc);

            IList<Element> doors = collector.WherePasses(doorInstanceFilter).ToElements();
            return doors;
        }
    }
}