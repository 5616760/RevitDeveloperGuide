using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace _009GetAElement {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Element selElement = null;
            foreach (ElementId id in uiDoc.Selection.GetElementIds()) {
                selElement = doc.GetElement(id);
                break;
            }

            Category category = selElement.Category;
            BuiltInCategory enumCategory = (BuiltInCategory)category.Id.IntegerValue;
            TaskDialog.Show("ok", enumCategory.ToString(), TaskDialogCommonButtons.Ok);//OST_StarRailing
            return Result.Succeeded;
        }
    }
}