using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace _003SelectElements {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            try {
                Selection selection = uiDoc.Selection;
                ICollection<ElementId> seleId = selection.GetElementIds();
                if (seleId.Count == 0) {
                    TaskDialog.Show("Error", "你还没有选择任何物体！");
                }
                else {
                    string info = "选择对象的ID是：";
                    foreach (ElementId id in seleId) {
                        info += "\n\t" + id.IntegerValue;
                    }

                    TaskDialog.Show("Revit", info);
                }
            }
            catch (Exception e) {
                message = e.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}