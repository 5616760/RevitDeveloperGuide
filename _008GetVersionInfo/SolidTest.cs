using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace _008GetVersionInfo {
    public class SolidTest {
        //判断当前版本
        public bool IsSuported(Application app) {
            if (app.VersionNumber == "2020" && string.Compare(app.VersionBuild, "20190201") > 0) {
                return true;
            }
            else {
                TaskDialog dialog = new TaskDialog("不支持") {
                    MainIcon = TaskDialogIcon.TaskDialogIconWarning,
                    MainInstruction = "此应用不支持当前版本Revit，请使用Revit2020或更高版本！"
                };
                dialog.Show();
                return false;
            }
        }
        /// <summary>
        /// 获取结构柱类别的默认族类型ID
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="column"></param>
        public void AssignDefaultTypeToColumn(Document doc, FamilyInstance column) {
            ElementId defaultTypeId = doc.GetDefaultFamilyTypeId(new ElementId(BuiltInCategory.OST_StructuralColumns));
            if (defaultTypeId != ElementId.InvalidElementId) {
                FamilySymbol defaultSymbol = doc.GetElement(defaultTypeId) as FamilySymbol;
                if (defaultSymbol != null) {
                    column.Symbol = defaultSymbol;
                }
            }
        }
        //将给定的门设置为默认类型
        public void SetDefaultTypeFromDoor(Document doc, FamilyInstance door) {
            ElementId doorCategoryId = new ElementId(BuiltInCategory.OST_Doors);
            //需要测试类型是否适合作为默认族类型，因为并非每个类型都可以设置为默认类型。
            //尝试设置非限定默认类型将导致异常
            if (door.Symbol.IsValidDefaultFamilyType(doorCategoryId)) {
                doc.SetDefaultFamilyTypeId(doorCategoryId, door.Symbol.Id);
            }
        }
        //检查给定墙是否使用墙类型的默认元素类型
        public bool IsWallUsingDefaultType(Document doc, Wall wall) {
            ElementId defaultElementTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.WallType);
            return wall.WallType.Id == defaultElementTypeId;
        }
        /// <summary>
        /// 从revit的内部单位转换
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public double GetYieldStressInKsi(Material material) {
            double dMinYieldStress = 0;
            ElementId strucAssetId = material.StructuralAssetId;
            if (strucAssetId != ElementId.InvalidElementId) {
                PropertySetElement pse = material.Document.GetElement(strucAssetId) as PropertySetElement;
                if (pse != null) {
                    StructuralAsset asset = pse.GetStructuralAsset();
                    dMinYieldStress = asset.MinimumYieldStress;
                    dMinYieldStress =
                        UnitUtils.ConvertFromInternalUnits(dMinYieldStress, DisplayUnitType.DUT_KIPS_PER_SQUARE_INCH);
                }
            }
            return dMinYieldStress;
        }
        /// <summary>
        /// 将单位由英寸改为英尺
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="dOffsetInches"></param>
        public void SetTopOffset(Wall wall, double dOffsetInches) {
            double dOffsetFeet = UnitUtils.Convert(dOffsetInches, DisplayUnitType.DUT_DECIMAL_INCHES,
                DisplayUnitType.DUT_DECIMAL_FEET);
            Parameter parameterTopFooset = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
            parameterTopFooset.Set(dOffsetFeet);
        }
        /// <summary>
        /// 获取单位转换后的材料密度
        /// </summary>
        /// <param name="material"></param>
        public void DisplayDensityOfMaterial(Material material) {
            double density = 0;
            ElementId strucAssetId = material.StructuralAssetId;
            if (strucAssetId != ElementId.InvalidElementId) {
                PropertySetElement pse = material.Document.GetElement(strucAssetId) as PropertySetElement;
                if (pse != null) {
                    StructuralAsset asset = pse.GetStructuralAsset();
                    density = asset.Density;
                    Units units = material.Document.GetUnits();
                    string strDensity = UnitFormatUtils.Format(units, UnitType.UT_UnitWeight, density, false, false);
                    string meg = string.Format($"原始值为:{density}\r\n转换后的值为：{strDensity}");
                    TaskDialog.Show("单位转换", meg);
                }
            }
        }
        /// <summary>
        /// 长度单位转换
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="userInputLength"></param>
        /// <returns></returns>
        public double GetLengthInput(Document doc, string userInputLength) {
            Units units = doc.GetUnits();
            bool parsed = UnitFormatUtils.TryParse(units, UnitType.UT_Length, userInputLength, out double dParsedLength);
            if (parsed == true) {
                string msg = string.Format($"用户输入的值为：{userInputLength}\n\t转换后的值为：{dParsedLength}");
                TaskDialog.Show("数据转换", msg);
            }
            return dParsedLength;
        }


    }
}