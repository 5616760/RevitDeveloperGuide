using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Structure;

namespace _011EleFilter {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SolidTest : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            LogicalAndFilterTest(doc);
            return Result.Succeeded;
        }

        public static void LogicalAndFilterTest(Document doc)
        {
            //情形1：合并两个过滤器-找到所有符合特定设计选型的墙
            ElementClassFilter wallFilter=new ElementClassFilter(typeof(Wall));
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<ElementId> designOptionIds = collector.OfClass(typeof(DesignOption)).ToElementIds();
            int n = 0;
            foreach (ElementId id in designOptionIds)
            {
                ElementDesignOptionFilter designfFilter=new ElementDesignOptionFilter(id);
                LogicalAndFilter andFilter=new LogicalAndFilter(wallFilter,designfFilter);
                collector=new FilteredElementCollector(doc);
                int wallCount = collector.WherePasses(andFilter).ToElements().Count;
                n += wallCount;
            }

            foreach (ElementId id in designOptionIds)
            {
                List<ElementFilter> filters=new List<ElementFilter>();
                filters.Add(wallFilter);
                filters.Add(new ElementDesignOptionFilter(id));
                filters.Add(new StructuralWallUsageFilter(StructuralWallUsage.Bearing));
                LogicalAndFilter andFilter=new LogicalAndFilter(filters);

                collector=new FilteredElementCollector(doc);
                int wallCount= collector.WherePasses(andFilter).ToElements().Count;

                n += wallCount;
            }
            TaskDialog.Show("T", n.ToString());
        }
        public static void LogicalOrFilterTest(Document doc)
        {
            //情形1：合并两个过滤器-找到所有属于墙或者属于标高类别的元素
            ElementCategoryFilter filterWall=new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementCategoryFilter filterLevel=new ElementCategoryFilter(BuiltInCategory.OST_Levels);
            LogicalOrFilter orFilter=new LogicalOrFilter(filterWall,filterLevel);
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<Element> founds= collector.WherePasses(orFilter).ToElements();
            int n = 0;
            foreach (Element element in founds)
            {
                n++;
            }
            //情形2：合并两个过滤器集合-找到所有属于传入类型的元素
            Type[] elemTypes = {typeof(Wall), typeof(Level), typeof(Floor), typeof(Rebar), typeof(MEPSystem)};
            List<ElementFilter> filters=new List<ElementFilter>();
            foreach (Type elemType in elemTypes)
            {
                ElementClassFilter filter=new ElementClassFilter(elemType);
                filters.Add(filter);
            }
            orFilter=new LogicalOrFilter(filters);
            collector=new FilteredElementCollector(doc);
            founds = collector.WherePasses(orFilter).ToElements();
            foreach (Element element in founds) {
                n++;
            }
            TaskDialog.Show("T", n.ToString());
        }
        /// <summary>
        /// CurveElementFilter匹配线型元素
        /// </summary>
        /// <param name="doc"></param>
        public static void CEFT(Document doc)
        {
            int n = 0;
            Array stTypes = Enum.GetValues(typeof(CurveElementType));
            foreach (CurveElementType t in stTypes)
            {
                if (t==CurveElementType.Invalid)
                {
                    continue;
                }
                FilteredElementCollector collector=new FilteredElementCollector(doc);
                CurveElementFilter filter=new CurveElementFilter(t);
                var founds = collector.WherePasses(filter).ToElementIds().Count;
                n += founds;
            }
            TaskDialog.Show("T", n.ToString());

        }
        /// <summary>
        /// FamilyInstanceFilter通过族类型来过滤实例
        /// </summary>
        /// <param name="doc"></param>
        public static void FIFT(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            collector = collector.OfClass(typeof(FamilySymbol));
            var q = collector.First(m => m.Name == "矩形柱600X600");
            ElementId symbolId = q.Id;
            collector=new FilteredElementCollector(doc);
            FamilyInstanceFilter filter=new FamilyInstanceFilter(doc,symbolId);
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();

            TaskDialog.Show("T", founds.Count.ToString());

        }
        /// <summary>
        /// ElementParameterFilter使用参数过滤器
        /// </summary>
        /// <param name="doc"></param>
        public static void EPFT(Document doc)
        {
            BuiltInParameter testPara = BuiltInParameter.ID_PARAM;
            ParameterValueProvider pvp=new ParameterValueProvider(new ElementId((int)testPara));
            FilterNumericRuleEvaluator fnre=new FilterNumericLessOrEqual();
            ElementId ruleValId=new ElementId(10);
            FilterRule fRule=new FilterElementIdRule(pvp,fnre,ruleValId);
            ElementParameterFilter filter=new ElementParameterFilter(fRule);
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();
            foreach (Element element in founds)
            {
                TaskDialog.Show("T", $"Element Id:{element.Id.IntegerValue}");
            }
        }
        /// <summary>
        /// ElementLevelFilter
        /// </summary>
        /// <param name="doc"></param>
        public static void ELFT(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<ElementId> levelisIds = collector.OfClass(typeof(Level)).ToElementIds();
            foreach (ElementId id in levelisIds)
            {
                collector=new FilteredElementCollector(doc);
                ElementFilter filter=new ElementLevelFilter(id);
                ICollection<ElementId> founds = collector.WherePasses(filter).ToElementIds();
                TaskDialog.Show("T", $"{founds.Count}Elements are associated to Level{id.IntegerValue}");
            }
        }
        /// <summary>
        /// ExclusionFilter去除掉已经过滤的元素
        /// </summary>
        /// <param name="doc"></param>
        public static void EFT(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<ElementId> excludes = collector.OfClass(typeof(FamilySymbol)).ToElementIds();
            ExclusionFilter filter=new ExclusionFilter(excludes);
            ICollection<ElementId> foundIds = collector.WhereElementIsElementType().WherePasses(filter).ToElementIds();
            TaskDialog.Show("T", $"Found {foundIds.Count} ElementTpyes which are not FamilySybmols");
        }
        /// <summary>
        /// FamilySymbolFilter使用传入的族，获取所有族类型
        /// </summary>
        /// <param name="doc"></param>
        public static void FSF(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ICollection<ElementId> foundIds = collector.OfClass(typeof(Family)).ToElementIds();
            int n = 0;
            foreach (ElementId id in foundIds)
            {
                collector=new FilteredElementCollector(doc);
                FamilySymbolFilter filter=new FamilySymbolFilter(id);
                ICollection<ElementId> f = collector.WherePasses(filter).ToElementIds();
                n += f.Count;
            }
            TaskDialog.Show("T", n.ToString());
        }
        /// <summary>
        /// ElementIsElementTypeFilter,过滤出元素类型
        /// </summary>
        /// <param name="doc"></param>
        public static void EIETF(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ElementFilter filter=new ElementIsElementTypeFilter();
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();
            TaskDialog.Show("T", founds.Count.ToString());
        }
        /// <summary>
        /// ElementClassFilter
        /// </summary>
        /// <param name="doc"></param>
        public static void ElementClassFilterTest(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ElementFilter filter=new ElementClassFilter(typeof(Wall));
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();
            
            TaskDialog.Show("tt", founds.Count.ToString());
        }
        /// <summary>
        /// 使用ElementCategoryFilter过来元素
        /// </summary>
        /// <param name="doc"></param>
        public static void ElementCategoryFilterTest(Document doc)
        {
            FilteredElementCollector collector=new FilteredElementCollector(doc);
            ElementFilter filter=new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ICollection<Element> founds = collector.WherePasses(filter).ToElements();
            TaskDialog.Show("tt", founds.Count.ToString());
        }

        public static void LevelFilterTest(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_StackedWalls);
            collector.WherePasses(filter).OfClass(typeof(Wall));
            ICollection<Element> found = collector.ToElements();
            foreach (Element ele in found)
            {
                TaskDialog.Show("T", ele.Name);
            }

            FilteredElementCollector collectorLev = new FilteredElementCollector(doc);
            //ElementFilter levFilter=new ElementClassFilter(typeof(Level));
            collectorLev = collectorLev.OfCategory(BuiltInCategory.OST_Levels);
            Level levs1 = collectorLev.First(m => m.Name == "标高 1") as Level; //通过名称引用
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

            if (levs1 != null)
            {
                TaskDialog.Show("Lev", levs1.Name.ToString() + "\n\t" + (levs1.Id.IntegerValue).ToString());
            }
        }
    }
}