using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace _009GetAElement {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Application app = commandData.Application.Application;
            Document doc = uiDoc.Document;
            ArrayCreate(doc);
            //MoveColumn(doc);
            //CteareDoor(doc);
            //string s = CreateDefinition(doc);
            //foreach (string id in GetDefinition(app))
            //{
            //    s += "\n" + id;
            //}

            //Parameter p= selElement.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

            //Category category = selElement.Category;
            //BuiltInCategory enumCategory = (BuiltInCategory)category.Id.IntegerValue;

            //TaskDialog.Show("ok", s, TaskDialogCommonButtons.Ok);//OST_StarRailing
            return Result.Succeeded;
        }
        /// <summary>
        /// 3.2.6阵列
        /// </summary>
        /// <param name="doc"></param>
        public void ArrayCreate(Document doc)
        {
            Wall wall=doc.GetElement(new ElementId(338393)) as Wall;
            using (Transaction tr=new Transaction(doc,"阵列"))
            {
                tr.Start();
                XYZ translation=new XYZ(0,10,0);
                //LinearArray.Create(doc, doc.ActiveView, wall.Id, 4, translation, ArrayAnchorMember.Last);
                //RadialArray.Create(doc, doc.ActiveView, wall.Id, 16, Line.CreateBound(new XYZ(), new XYZ(0, 0, 1)),
                //    Math.PI , ArrayAnchorMember.Last);
                RadialArray.ArrayElementWithoutAssociation(doc, doc.ActiveView, wall.Id, 6, Line.CreateBound(new XYZ(), new XYZ(0, 0, 1)),
                    Math.PI/3, ArrayAnchorMember.Second);//阵列但不成组
                tr.Commit();
            }
        }

        /// <summary>
        /// 已知ID时获取对象
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="id"></param>
        public void GetElenet(Document doc, ElementId id) {
            Element ele = doc.GetElement(id);
            Level level = ele as Level;
            if (level != null) {
                //使用
            }
        }
        /// <summary>
        /// 获取内墙的厚度
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public string GetElenet(Document doc) {
            //FilteredElementCollector filteredElements=new FilteredElementCollector(doc);
            //ElementClassFilter classFilter=new ElementClassFilter(typeof(Level));
            //ElementCategoryFilter categoryFilter=new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            //filteredElements.WherePasses(classFilter).WherePasses(categoryFilter);
            //string s = "所有标高：";
            //foreach (Element e in filteredElements)
            //{
            //    s += "\n\t" + e.Location.ToString();
            //}
            //return s;

            string s = "";
            FilteredElementCollector filteredElements = new FilteredElementCollector(doc);
            ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(Wall));
            filteredElements = filteredElements.WherePasses(elementClassFilter);
            foreach (Wall wall in filteredElements) {
                Parameter functionParameter = wall.WallType.get_Parameter(BuiltInParameter.FUNCTION_PARAM);
                if (functionParameter != null && functionParameter.StorageType == StorageType.Integer) {
                    s += "\n\t" + wall.Width * 304.8;
                }
            }
            return s;
        }
        /// <summary>
        /// 获取对象中的指定参数
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public double GetLen(Element element) {
            double l = 0;
            ParameterSet parameters = element.Parameters;
            foreach (Parameter p in parameters) {
                if (p.Definition.Name == "长度" && p.StorageType == StorageType.Double) {
                    l = p.AsDouble() * 304.8;
                    break;
                }
            }

            return l;
        }
        /// <summary>
        /// 获取共享参数
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public List<string> GetDefinition(Application app) {
            List<string> sList = new List<string>();
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            if (definitionFile == null) {
                sList.Add("未找到共享文件！");
                return sList;
            }
            DefinitionGroups definitionGroups = definitionFile.Groups;
            foreach (DefinitionGroup definitionGroup in definitionGroups) {
                foreach (Definition d in definitionGroup.Definitions) {
                    sList.Add(d.Name);
                }
            }

            return sList;
        }

        public string CreateDefinition(Document doc) {
            string shareParameterFilename = @"C:\Users\Administrator\Desktop\fff.txt";
            string groupName = "MyGroup";
            string definitionName = "Mydefinition";
            ParameterType parameterType = ParameterType.Text;
            CategorySet categorySet = new CategorySet();
            Category wallCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
            categorySet.Insert(wallCategory);
            bool instanceParameter = true;
            BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.PG_DATA;
            if (!File.Exists(shareParameterFilename)) {
                try {
                    StreamWriter sw = File.CreateText(shareParameterFilename);
                    sw.Close();
                }
                catch (Exception) {
                    throw new Exception("不能创建共享文件：" + shareParameterFilename);
                }
            }
            //设置共享参数文件
            doc.Application.SharedParametersFilename = shareParameterFilename;
            //打开共享参数文件
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            if (definitionFile == null) {
                throw new Exception("未找到共享文件！");
            }
            //获取参数组的集合
            DefinitionGroups groups = definitionFile.Groups;
            DefinitionGroup group = groups.get_Item(groupName);
            if (group == null) {
                group = groups.Create(groupName);
            }

            if (group == null) {
                throw new Exception("创建参数组失败：" + groupName);
            }
            //获取参数定义
            Definition definition = group.Definitions.get_Item(definitionName);
            if (definition == null) {
                ExternalDefinitionCreationOptions op = new ExternalDefinitionCreationOptions(definitionName, parameterType);
                definition = group.Definitions.Create(op);
            }
            //调用不同的函数创建类型参数或实例参数
            ElementBinding binding = null;
            if (instanceParameter) {
                binding = doc.Application.Create.NewInstanceBinding(categorySet);
            }
            else {
                binding = doc.Application.Create.NewTypeBinding(categorySet);
            }
            //把参数定义和类别绑定起来，元素的新的参数就创建成功了
            try {
                doc.ParameterBindings.Insert(definition, binding, parameterGroup);
            }
            catch (Exception) {
                throw new Exception("绑定失败");
            }

            return shareParameterFilename;
        }
        /// <summary>
        /// 3-11:获取分析模型的几何信息
        /// </summary>
        /// <param name="doc"></param>
        public void GetModelGeotry(Document doc) {
            Element element = doc.GetElement(new ElementId(338537));
            if (element == null) {
                return;
            }

            AnalyticalModel analysisMode = element.GetAnalyticalModel();
            if (analysisMode.IsSingleCurve()) {
                Curve c = analysisMode.GetCurve();
            }
            else if (analysisMode.IsSinglePoint()) {
                XYZ p = analysisMode.GetPoint();
            }

            else {
                IList<Curve> curves = analysisMode.GetCurves(AnalyticalCurveType.ActiveCurves);
            }
        }
        /// <summary>
        /// 3-12:放置门
        /// </summary>
        /// <param name="doc"></param>
        public void CteareDoor(Document doc) {
            string doorTypeName = "900 x 2100mm";
            FamilySymbol doorType = null;
            //在文档中查找名为“900 x 2100mm”的门
            ElementFilter doorCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            ElementFilter familySymbolFilter = new ElementClassFilter(typeof(FamilySymbol));
            LogicalAndFilter andFilter = new LogicalAndFilter(doorCategoryFilter, familySymbolFilter);
            FilteredElementCollector doorSymbol = new FilteredElementCollector(doc);
            doorSymbol.WherePasses(andFilter);
            bool symbolFound = false;
            foreach (FamilySymbol element in doorSymbol) {
                if (element.Name == doorTypeName) {
                    symbolFound = true;
                    doorType = element;
                    break;
                }
            }
            //如果没找到

            if (!symbolFound) {
                using (Transaction tr1 = new Transaction(doc, "载入族"))
                {
                    tr1.Start();
                    string file = @"C:\ProgramData\Autodesk\RVT 2020\Libraries\China\建筑\门\普通门\平开门\单扇\单嵌板木门 5.rfa";
                    bool loadSuccess = doc.LoadFamily(file, out Family family);
                    if (loadSuccess) {
                        foreach (ElementId id in family.GetValidTypes()) {
                            doorType = doc.GetElement(id) as FamilySymbol;
                            if (doorType != null) {
                                if (doorType.Name == doorTypeName) {
                                    break;
                                }
                            }
                        }
                    }
                    else {
                        TaskDialog.Show("加载失败", "未能成功加载族：\n" + file);
                    }

                    tr1.Commit();
                }
            }

            //使用族类型创建门
            if (doorType != null) {
                ElementFilter wallFilter = new ElementClassFilter(typeof(Wall));
                FilteredElementCollector filteredElement = new FilteredElementCollector(doc);
                filteredElement = filteredElement.WherePasses(wallFilter);
                IList<Wall> walls = new List<Wall>();
                IList<Line> lines = new List<Line>();
                foreach (Wall wall1 in filteredElement) {
                    LocationCurve locationCurve = wall1.Location as LocationCurve;
                    if (locationCurve != null) {
                        Line line = locationCurve.Curve as Line;
                        if (line != null) {
                            walls.Add(wall1);
                            lines.Add(line);
                        }
                    }
                }

                //在墙的中心位置创建一个门
                if (walls.Count > 0) {
                    using (Transaction tr = new Transaction(doc, "创建门")) {
                        tr.Start();
                        string prompt = "门的ID是：";
                        for (int i = 0; i < walls.Count; i++) {
                            XYZ midXyz = (lines[i].GetEndPoint(0) + lines[i].GetEndPoint(1)) / 2;
                            Level level = doc.GetElement(walls[i].LevelId) as Level;
                            //创建门：传入标高参数，作为门的默认标高

                            FamilyInstance door = doc.Create.NewFamilyInstance(midXyz, doorType, walls[i], level,
                                StructuralType.NonStructural);
                            prompt += "\n\t" + door.Id.ToString();
                        }
                        tr.Commit();
                        TaskDialog.Show("Success", prompt);
                        Trace.WriteLine(prompt);
                    }
                }
                else {
                    TaskDialog.Show("元素不存在", "没有找到符合条件的墙");
                }
            }
            else {
                TaskDialog.Show("族类型不存在", "没有找到族类型：" + doorTypeName);
            }
        }

        public void MoveColumn(Document doc)
        {
            using (Transaction tr=new Transaction(doc,"移动柱"))
            {
                tr.Start();
                var creater = doc.Create;
                XYZ origin=new XYZ();
                Level level = GetALevel(doc);
                FamilySymbol columnSymbol = GetAType(doc, BuiltInCategory.OST_StructuralColumns, "矩形柱600X600");
                FamilyInstance column = creater.NewFamilyInstance(origin, columnSymbol, level, StructuralType.Column);
                XYZ newPlace=new XYZ(10,20,30);
                if (!column.Pinned)//移动之前需要判断是否被锁定
                {
                    ElementTransformUtils.CopyElement(doc,column.Id,newPlace);
                    LocationPoint cpt=column.Location as LocationPoint;//通过定位点移动
                    cpt.Move(new XYZ(10, 10, 0));
                    cpt.Point=new XYZ(0,20,0);
                    cpt.Rotate(Line.CreateBound(cpt.Point, new XYZ(cpt.Point.X, cpt.Point.Y, 1)), Math.PI / 3);
                    if (ElementTransformUtils.CanMirrorElement(doc,column.Id))//镜像之前应进行判断
                    {
                        Plane plane=Plane.CreateByNormalAndOrigin(XYZ.BasisY, new XYZ(0,2,0));//XYZ.BasisY沿X轴镜像
                        ElementTransformUtils.MirrorElement(doc,column.Id,plane);
                    }

                    TaskDialog.Show("Mir", column.Mirrored.ToString());
                }
                else
                {
                    TaskDialog.Show("错误","对象被锁定，无法移动",TaskDialogCommonButtons.Ok);
                }
                tr.Commit();
            }
        }

        private FamilySymbol GetAType(Document doc, BuiltInCategory bc,string name) {
            ElementClassFilter familySymbolFilter = new ElementClassFilter(typeof(FamilySymbol));
            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(bc);
            LogicalAndFilter columnInstanceFilter = new LogicalAndFilter(familySymbolFilter, categoryFilter);
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            FamilySymbol cc = null;
            var columns = collector.WherePasses(columnInstanceFilter);
            foreach (Element element in collector)
            {
                if (element.Name== name)
                {
                    cc=element as FamilySymbol;
                }
            }
            return cc;
        }

        private Level GetALevel(Document doc)
        {
            Level l = doc.GetElement(new ElementId(311)) as Level;
            return l;
        }

        public void RotateWall(Document doc)
        {
            Level level= GetALevel(doc);
            using (Transaction tr=new Transaction(doc,"旋转墙"))
            {
                tr.Start();
                Line line1=Line.CreateBound(new XYZ(0,-10,0), new XYZ(0,10,0));
                Line line2=Line.CreateBound(new XYZ(0, 10, 0), new XYZ(20, 10, 0));
                Wall wall1=Wall.Create(doc,line1, level.Id,false);
                Wall wall2=Wall.Create(doc,line2, level.Id,false);
                LocationCurve wCurve1=wall1.Location as LocationCurve;
                XYZ pt1 = wCurve1.Curve.GetEndPoint(1);
                XYZ pt2=new XYZ(pt1.X,pt1.Y,1);
                Line axis=Line.CreateBound(pt1,pt2);
                ElementTransformUtils.RotateElement(doc,wall1.Id,axis,Math.PI/4);//使用工具旋转墙
                wCurve1.Rotate(axis, Math.PI / 2);//旋转定位线
                tr.Commit();
            }
        }

        private WallType GetWallType(Document doc,  string name) {
            ElementClassFilter wallTypeFilter = new ElementClassFilter(typeof(WallType));
            FilteredElementCollector collector = new FilteredElementCollector(doc);

            WallType cc = null;
            var columns = collector.WherePasses(wallTypeFilter);
            foreach (Element element in collector) {
                if (element.Name == name) {
                    cc = element as WallType;
                    break;
                }
            }
            return cc;
        }
    }
}