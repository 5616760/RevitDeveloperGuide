using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace _005HighlightingWalls {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //message = "请注意高亮显示的墙！";
            //FilteredElementCollector collector=new FilteredElementCollector(doc);
            //ICollection<Element> collection = collector.OfClass(typeof(Wall)).ToElements();
            //foreach (Element element in collection)
            //{
            //    elements.Insert(element);
            //}
            //return Result.Failed;
            try
            {
                ICollection<ElementId> ids=new List<ElementId>();
                TaskDialog taskDialog=new TaskDialog("Revit");
                taskDialog.MainContent = "点击确认返回成功，选择的对象将被删除。\n" +
                                         "点击NO返回失败，选择的对象不会被删除。\n" +
                                         "点击取消返回取消，选择的对象不会被删除。";
                TaskDialogCommonButtons buttons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No |
                                                  TaskDialogCommonButtons.Cancel;
                taskDialog.CommonButtons = buttons;
                TaskDialogResult tdr = taskDialog.Show();
                if (tdr==TaskDialogResult.Yes)
                {
                    return Result.Succeeded;
                }
                else if (tdr==TaskDialogResult.No)
                {
                    ICollection<ElementId> sElementIds = uiDoc.Selection.GetElementIds();
                    foreach (ElementId id in sElementIds)
                    {
                        elements.Insert(doc.GetElement(id));
                    }
                    return Result.Failed;
                }
                else
                {
                    return Result.Cancelled;
                }
            }
            catch
            {
                message = "发生异常";
                return Result.Failed;

            }

        }
    }
    /// <summary>
    /// 允许在没有选择或至少选择一堵墙时单击按钮：
    /// </summary>
    public class SampleAccessibilityCheck : IExternalCommandAvailability {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories) {
            if (selectedCategories.IsEmpty)
            {
                return true;
            }

            foreach (Category c in selectedCategories)
            {
                if (c.Id.IntegerValue == (int) BuiltInCategory.OST_Walls)
                    return true;
            }

            return false;
        }
    }
}
