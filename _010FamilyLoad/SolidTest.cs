using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace _010FamilyLoad {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //Element elem = doc.GetElement(new ElementId(338434));
            //FamilyInstance famInst = elem as FamilyInstance;
            //Document famDoc = doc.EditFamily(famInst.Symbol.Family);
            //using (Transaction tr = new Transaction(famDoc, "编辑族")) {
            //    tr.Start();
            //    string paramName = "MyPara14";
            //    famDoc.FamilyManager.AddParameter(paramName, BuiltInParameterGroup.PG_TEXT, ParameterType.Text, false);
            //    //CreateReferenceplane(famDoc);
            //    tr.Commit();
            //}
            if (doc.IsFamilyDocument)
            {
                string s= GetSketchFromExtrusion(doc);
                TaskDialog.Show("ss", s);
            }
            else
            {
                TaskDialog.Show("xx", "OnlyInFamile");
            }
            //Family loadedFamily = famDoc.LoadFamily(doc, new ProjectFamilyLoadOption());
            return Result.Succeeded;
        }

        public string GetSketchFromExtrusion(Document doc)
        {
            Extrusion extrusion=doc.GetElement(new ElementId(2760)) as Extrusion;
            SketchPlane sketchPlane = extrusion.Sketch.SketchPlane;
            CurveArrArray sktchProfile = extrusion.Sketch.Profile;
            return (extrusion.EndOffset*304.8).ToString();
        }
        /// <summary>
        /// 在族中创建曲线
        /// </summary>
        /// <param name="doc"></param>
        public void CreateSketchPlaneByPlane(Document doc)
        {
            using (Transaction tr=new Transaction(doc,"创建曲线"))
            {
                tr.Start();
                Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ,XYZ.Zero);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                Arc arc=Arc.Create(plane,5,0,Math.PI*2);
                ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(arc, sketchPlane);
                tr.Commit();
            }
        }
        /// <summary>
        /// 用标高创建参考平面
        /// </summary>
        /// <param name="doc"></param>
        public void CreateModelCurve(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            collector = collector.OfCategory(BuiltInCategory.OST_Levels);
            var levelEle = collector.Where(m => m.Name == "参照标高");
            List<Element> levList = levelEle.ToList();
            if (levList.Count<1)
            {
                return;
            }

            Level refLevel = levList[0] as Level;
            using (Transaction tr=new Transaction(doc,"创建直线"))
            {
                tr.Start();
                Line line=Line.CreateBound(XYZ.Zero, new XYZ(10,10,0));
                SketchPlane sketchPlane=SketchPlane.Create(doc,refLevel.Id);
                ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                tr.Commit();
            }
        }
        /// <summary>
        /// 将模型线转换为参考线，只能在族中使用
        /// </summary>
        /// <param name="doc"></param>
        public void ChangeModelCurveToReferenceLine(Document doc)
        {
            ModelCurve modelCurve=doc.GetElement(new ElementId(339205)) as ModelCurve;
            using (SubTransaction str=new SubTransaction(doc))
            {
                str.Start();
                modelCurve.ChangeToReferenceLine();
                str.Commit();
            }
        }
        /// <summary>
        /// 在族中创建参照平面
        /// </summary>
        /// <param name="doc"></param>
        public void CreateReferenceplane(Document doc) {
            if (!doc.IsFamilyDocument) {
                return;
            }

            using (SubTransaction tr1 = new SubTransaction(doc)) {
                tr1.Start();
                XYZ bubbleEnd = new XYZ(0, 5, 5);
                XYZ freeEnd = new XYZ(5, 5, 5);
                XYZ cutVector = XYZ.BasisY;
                View view = doc.ActiveView;
                ReferencePlane referencePlane = doc.FamilyCreate.NewReferencePlane(bubbleEnd, freeEnd, cutVector, view);
                referencePlane.Name = "MyReferencePlane";

                tr1.Commit();
            }
        }
    }

    public class ProjectFamilyLoadOption : IFamilyLoadOptions {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) {
            source = FamilySource.Project;
            overwriteParameterValues = true;
            return true;
        }
    }
}