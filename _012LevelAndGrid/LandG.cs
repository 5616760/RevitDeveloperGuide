using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;

namespace _012LevelAndGrid {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class LandG : IExternalCommand {
        public static Document _doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            _doc = doc;
            //Level l= CreateLevel( 8,"标高 6");
            //GetWallLayer(doc);
            //ViewPlan viewPlan=ViewPlan.Create(_doc,new ElementId(49552), new ElementId(311));
            //AreaCreate2(viewPlan);
            CreateOpening();
            return Result.Succeeded;
        }

        public static void CreateOpening()
        {

            Wall wall = GetElement<Wall>(338564);
            LocationCurve locationCurve=wall.Location as LocationCurve;
            Line line=locationCurve.Curve as Line;
            XYZ startPoint = line.GetEndPoint(0);
            XYZ endPoint = line.GetEndPoint(1);
            Parameter wallHeightParameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            double wallHeight = wallHeightParameter.AsDouble();
            XYZ delta = (endPoint - startPoint + new XYZ(0, 0, wallHeight)) / 3;
            using (Transaction tr=new Transaction(_doc))
            {
                tr.Start("开洞");
                Opening opening = _doc.Create.NewOpening(wall, startPoint + delta, startPoint + delta * 2);
                tr.Commit();
            }
        }
        /// <summary>
        /// 获取洞口边界
        /// </summary>
        public static void GetOpeningBoundary()
        {
            Opening opening = GetElement<Opening>(338508);
            Element h= opening.Host;
            TaskDialog.Show("T", h.Id.ToString());
            if (opening.IsRectBoundary)
            {
                XYZ p1 = opening.BoundaryRect[0];
                XYZ p2 = opening.BoundaryRect[1];
                TaskDialog.Show("T", $"起点坐标：{p1.ToString()}\n\t终点坐标：{p2.ToString()}");
            }
            else
            {
                double l = 0;
                foreach (Curve curve in opening.BoundaryCurves)
                {
                    l += curve.Length;
                }

                TaskDialog.Show("T", $"边界总长度为{(l*304.8).ToString()}");
            }
        }
        /// <summary>
        /// 创建一条样条曲线
        /// </summary>
        public static void CreateSpline()
        {
            using (Transaction tr=new Transaction(_doc))
            {
                tr.Start("Create Spline");
                SketchPlane sketchPlane=SketchPlane.Create(_doc,Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                NurbSpline nurbSpline=NurbSpline.CreateCurve(new List<XYZ>{new XYZ(),new XYZ(10,0,0),new XYZ(10,10,0),new XYZ(20,10,0),new XYZ(20,20,0)},new List<double>{0.5,0.1,0.3,0.6,0.8}) as NurbSpline;
                ModelCurve modelCurve = _doc.Create.NewModelCurve(nurbSpline, sketchPlane);
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建模型线
        /// </summary>
        public static void CreateLine()
        {
            using (Transaction tr=new Transaction(_doc))
            {
                tr.Start("Create Line");
                Line geoLine=Line.CreateBound(XYZ.BasisY*20, XYZ.BasisX *10);
                Plane plane=Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                SketchPlane modelSketchPlane=SketchPlane.Create(_doc, plane);
                ModelCurve modelLine = _doc.Create.NewModelCurve(geoLine, modelSketchPlane);
                tr.Commit();
            }
        }
        /// <summary>
        /// 改变线样式，没看懂要干嘛
        /// </summary>
        public static void ChangeLineStyle()
        {
            ModelCurve modelCurve = null;
            ICollection<ElementId> styles = modelCurve.GetLineStyleIds();
            foreach (ElementId id in styles)
            {
                if (id != modelCurve.LineStyle.Id)
                {
                    using (Transaction tr=new Transaction(_doc))
                    {
                        tr.Start("Create Model Line");
                        modelCurve.LineStyle = _doc.GetElement(id) as GraphicsStyle;
                        tr.Commit();
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// 创建面积，暂时不成功
        /// </summary>
        /// <param name="areaView"></param>
        public static void AreaCreate2(ViewPlan areaView)
        {
            using (Transaction tr=new Transaction(_doc))
            {
                var create = _doc.Create;
                tr.Start("创建面积边界");
                var sketchPlane = areaView.SketchPlane;
                create.NewAreaBoundaryLine(sketchPlane, Line.CreateBound(new XYZ(20, 20, 0), new XYZ(40, 20, 0)),
                    areaView);
                create.NewAreaBoundaryLine(sketchPlane, Line.CreateBound(new XYZ(40, 20, 0), new XYZ(40, 40, 0)),
                    areaView);
                create.NewAreaBoundaryLine(sketchPlane, Line.CreateBound(new XYZ(40, 20, 0), new XYZ(20, 40, 0)),
                    areaView);
                create.NewAreaBoundaryLine(sketchPlane, Line.CreateBound(new XYZ(20, 40, 0), new XYZ(20, 20, 0)),
                    areaView);
                tr.Commit();
                tr.Start("Create Area");
                Area area = create.NewArea(areaView, new UV(2, 2));
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建面积，暂时不成功
        /// </summary>
        public static void AreaCreate() {
            Level level = _doc.GetElement(new ElementId(311)) as Level;
            //获取基于标高Level的一个视图
            var defaultView = new FilteredElementCollector(_doc).WherePasses(new ElementClassFilter(typeof(ViewPlan)))
                .Cast<ViewPlan>().FirstOrDefault();
            using (Transaction tr=new Transaction(_doc))
            {
                tr.Start("Area");
                Area area = _doc.Create.NewArea(defaultView, new UV(0.0, 0.0));
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建房间
        /// </summary>
        public static void RoomCteate()
        {
            Phase phase=_doc.GetElement(new ElementId(0)) as Phase;
            Level level = _doc.GetElement(new ElementId(311)) as Level;
            //获取基于标高Level的一个视图
            var defaultView = new FilteredElementCollector(_doc).WherePasses(new ElementClassFilter(typeof(View)))
                .Cast<View>().Where(v => v.GenLevel != null && v.GenLevel.Id == level.Id).FirstOrDefault();
            if (defaultView != null)
            {
                var defaultPhase = defaultView.get_Parameter(BuiltInParameter.VIEW_PHASE);
                if (defaultPhase != null && defaultPhase.AsElementId() == phase.Id)
                {
                    using (Transaction tr=new Transaction(_doc))
                    {
                        tr.Start("Get_PlanTopology");
                        var circuits = _doc.get_PlanTopology(level, phase).Circuits;
                        foreach (PlanCircuit planCircuit in circuits)
                        {
                            _doc.Create.NewRoom(null, planCircuit);
                        }
                        tr.Commit();
                    }

                    TaskDialog.Show("T", "创建成功");
                }
            }
        }
        /// <summary>
        /// 创建迹线屋顶
        /// </summary>
        public static void RoofCreate2()
        {
            using (Transaction tr=new Transaction(_doc))
            {
                //创建屋顶前准备参数
                Level level = _doc.GetElement(new ElementId(694)) as Level;
                RoofType roofType = _doc.GetElement(new ElementId(335)) as RoofType;
                CurveArray curveArray=new CurveArray();
                //屋顶外框
                curveArray.Append(Line.CreateBound(new XYZ(), new XYZ(30,0,0)));
                curveArray.Append(Line.CreateBound(new XYZ(30, 0, 0), new XYZ(30,30,0)));
                curveArray.Append(Line.CreateBound(new XYZ(30, 30, 0), new XYZ(0,30,0)));
                curveArray.Append(Line.CreateBound(new XYZ(0, 30, 0), new XYZ(0,0,0)));
                //在中间添加洞口
                curveArray.Append(Line.CreateBound(new XYZ(5,5,0), new XYZ(5,15,0)));
                curveArray.Append(Line.CreateBound(new XYZ(5, 15, 0), new XYZ(15,5,0)));
                curveArray.Append(Line.CreateBound(new XYZ(15,5,0), new XYZ(5,5,0)));

                //创建屋顶
                tr.Start("Create roof");
                ModelCurveArray modelCurveArray=new ModelCurveArray();
                FootPrintRoof roof = _doc.Create.NewFootPrintRoof(curveArray, level, roofType, out modelCurveArray);
                //设置屋顶坡度
                ModelCurve curve1 = modelCurveArray.get_Item(0);
                ModelCurve curve3 = modelCurveArray.get_Item(2);
                roof.set_DefinesSlope(curve1,true);
                roof.set_SlopeAngle(curve1,0.5);
                roof.set_DefinesSlope(curve3,true);
                roof.set_SlopeAngle(curve3,1.6);
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建拉伸屋面
        /// </summary>
        public static void RoofCreate()
        {
            View view = _doc.ActiveView;
            XYZ bubbleEnd=new XYZ();
            XYZ freeEnd=new XYZ(0,100,0);
            XYZ thirdPnt=new XYZ(0,0,100);
            using (Transaction tr = new Transaction(_doc ))
            {
                tr.Start("创建参考平面");
                ReferencePlane referencePlane = _doc.Create.NewReferencePlane2(bubbleEnd, freeEnd, thirdPnt, view);
                tr.Commit();
                Level level = _doc.GetElement(new ElementId(311)) as Level;
                RoofType roofType=_doc.GetElement(new ElementId(335)) as RoofType;
                CurveArray curveArray=new CurveArray();
                curveArray.Append(Line.CreateBound(new XYZ(0,0,50), new XYZ(0,50,100)));
                curveArray.Append(Line.CreateBound(new XYZ(0,50,100), new XYZ(0,100,50)));
                tr.Start("创建楼板");
                _doc.Create.NewExtrusionRoof(curveArray, referencePlane, level, roofType, 10, 200);
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建楼板
        /// </summary>
        public static void FloorCreate()
        {
            CurveArray curveArray=new CurveArray();
            curveArray.Append(Line.CreateBound(XYZ.Zero,new XYZ(100,0,0)));
            curveArray.Append(Line.CreateBound( new XYZ(100,0,0),new XYZ(0,100,0)));
            curveArray.Append(Line.CreateBound(new XYZ(0, 100, 0), new XYZ(0,0,0)));
            using (Transaction tr=new Transaction(_doc,"创建楼板"))
            {
                tr.Start();
                Floor floor = _doc.Create.NewFloor(curveArray, false);
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建带高度的墙
        /// </summary>
        public static void WallCreate4() {
            ElementId levelId = new ElementId(311);
            ElementId wallTypeId = new ElementId(22694);
            using (Transaction tr=new Transaction(_doc,"创建墙"))
            {
                tr.Start();
                Wall wall=Wall.Create(_doc,Line.CreateBound(XYZ.Zero, new XYZ(0,100,0)),wallTypeId,levelId,200,300,true,false);
                tr.Commit();
            }

        }
        /// <summary>
        /// 创建正反面墙
        /// </summary>
        public static void WallCreate3() {
            ElementId levelId = new ElementId(311);
            ElementId wallTypeId = new ElementId(22694);
            IList<Curve> curves = new List<Curve>();

            //创建第一面墙
            XYZ[] vertexes = new XYZ[] { XYZ.Zero, new XYZ(0, 100, 0), new XYZ(0, 0, 100) };
            for (int i = 0; i < vertexes.Length; i++) {
                if (i != vertexes.Length - 1) {
                    curves.Add(Line.CreateBound(vertexes[i], vertexes[i + 1]));
                }
                else {
                    curves.Add(Line.CreateBound(vertexes[i], vertexes[0]));
                }
            }

            Wall wall = null;
            using (Transaction tr = new Transaction(_doc, "创建墙1")) {
                tr.Start();
                wall = Wall.Create(_doc, curves, wallTypeId, levelId, false, new XYZ(-1, 0, 0));
                tr.Commit();
            }

            //创建第二面墙
            curves.Clear();
            vertexes = new XYZ[] { new XYZ(0, 0, 100), new XYZ(0, 100, 100), new XYZ(0, 100, 0) };

            for (int i = 0; i < vertexes.Length; i++) {
                if (i != vertexes.Length - 1) {
                    curves.Add(Line.CreateBound(vertexes[i], vertexes[i + 1]));
                }
                else {
                    curves.Add(Line.CreateBound(vertexes[i], vertexes[0]));
                }
            }

            using (Transaction tr = new Transaction(_doc, "创建墙2")) {
                tr.Start();
                wall = Wall.Create(_doc, curves, wallTypeId, levelId, false, new XYZ(1, 0, 0));
                tr.Commit();
            }

        }
        /// <summary>
        /// 创建多边形墙,曲线要闭合
        /// </summary>
        public static void WallCreate2() {
            IList<Curve> curves = new List<Curve> {
                Line.CreateBound(new XYZ(100, 20, 0), new XYZ(100, -20, 0)),
                Line.CreateBound(new XYZ(100, -20, 0), new XYZ(100, -10, 10)),
                Line.CreateBound(new XYZ(100, -10, 10), new XYZ(100, 10, 10)),
                Line.CreateBound(new XYZ(100, 10, 10), new XYZ(100, 20, 0))
            };
            using (Transaction tr = new Transaction(_doc, "创建多边形墙")) {
                tr.Start();
                Wall wall = Wall.Create(_doc, curves, false);
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建默认墙
        /// </summary>
        public static void WallCreate1() {
            ElementId levelId = new ElementId(311);
            using (Transaction tr = new Transaction(_doc, "创建墙")) {
                tr.Start();
                Wall wall = Wall.Create(_doc, Line.CreateBound(XYZ.Zero, new XYZ(30, 0, 0)), levelId, false);
                tr.Commit();
            }

        }
        /// <summary>
        /// 获取一个楼板朝上的面
        /// </summary>
        public void GetFaceOfFloor() {
            Floor floor = GetElement<Floor>(338379);
            IList<Reference> references = HostObjectUtils.GetTopFaces(floor);
            if (references.Count == 1) {
                Reference reference = references[0];
                GeometryObject topFaceGeo = floor.GetGeometryObjectFromReference(reference);
                PlanarFace topFace = topFaceGeo as PlanarFace;
                TaskDialog.Show("T", topFace.Area.ToString());
            }

        }
        /// <summary>
        /// 通过ID获取对象的泛型静态方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public static T GetElement<T>(int v) where T : Element {
            T ele = _doc.GetElement(new ElementId(v)) as T;
            return ele;
        }

        /// <summary>
        /// 获取墙各层厚度
        /// </summary>
        /// <param name="doc"></param>
        public static void GetWallLayer(Document doc) {
            Wall wall = doc.GetElement(new ElementId(364389)) as Wall;
            CompoundStructure compoundStructure = wall.WallType.GetCompoundStructure();
            if (compoundStructure == null) {
                return;
            }

            if (compoundStructure.LayerCount > 0) {
                foreach (CompoundStructureLayer layer in compoundStructure.GetLayers()) {
                    ElementId materialId = layer.MaterialId;
                    double layerWidth = layer.Width;
                    TaskDialog.Show("T", $"{(304.8 * layerWidth).ToString()}");
                }
            }
        }
        /// <summary>
        /// 创建轴网
        /// </summary>
        /// <param name="doc"></param>
        public static void GridCreate(Document doc) {
            using (Transaction tr = new Transaction(doc, "创建轴网")) {
                tr.Start();
                Grid grid = Grid.Create(doc, Line.CreateBound(XYZ.Zero, new XYZ(10, 10, 0)));
                grid.Name = "A";
                tr.Commit();
            }
        }
        /// <summary>
        /// 创建标高并创建相对应的视图
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elev"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Level CreateLevel(double elev, string name) {
            Level level = null;
            using (Transaction tr = new Transaction(_doc, "创建标高")) {
                tr.Start();
                level = Level.Create(_doc, elev / 0.3048);
                level.Name = name;
                tr.Commit();
            }

            ElementClassFilter classFilter = new ElementClassFilter(typeof(ViewFamilyType));
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            collector = collector.WherePasses(classFilter);
            foreach (ViewFamilyType type in collector) {
                if (type.ViewFamily == ViewFamily.FloorPlan || type.ViewFamily == ViewFamily.CeilingPlan) {
                    Transaction tr1 = new Transaction(_doc, $"创建{type}类型的视图");
                    tr1.Start();
                    ViewPlan view = ViewPlan.Create(_doc, type.Id, level.Id);
                    tr1.Commit();
                }
            }

            return level;
        }
    }
}