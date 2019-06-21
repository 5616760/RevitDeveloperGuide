using System.Collections.Generic;
using System.Net;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using _012LevelAndGrid;

namespace _013Dimension {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class DimTest : IExternalCommand {
        private static Document _doc = null;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            _doc = doc;

            return Result.Succeeded;
        }

        public static void CreateLineDim()
        {
            Wall wall = LandG.GetElement<Wall>(338564);
            using (Transaction tr = new Transaction(_doc)) {
                tr.Start("添加标注");
                Location location = wall.Location;
                LocationCurve locationLine=location as LocationCurve;
                if (locationLine == null)
                    return;
                Line newLine = null;
                ReferenceArray referenceArray=new ReferenceArray();
                tr.Commit();
            }
        }
    }
}