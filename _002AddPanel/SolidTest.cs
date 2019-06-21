using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace _002AddPanel {
    public class SolidTest : IExternalApplication {
        public Result OnShutdown(UIControlledApplication application) {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application) {
            //string tabName = "UCD场地工具";
            //application.CreateRibbonTab(tabName);
            //创建新面板
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Twinmotion", "UCD场地工具");
            //创建一个按钮添加到标签页
            string str = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButtonData = new PushButtonData("cmdHelloWorld", "创建轨道", @"D:\Studay\CSharp\Work\Revit\RailingCreat\bin\Release\RailingCreatV1.1.dll", "RailingCreat.RailingCreate");
            PushButton pushButton = ribbonPanel.AddItem(pushButtonData) as PushButton;
            //设置按钮属性
            //a、提示
            pushButton.ToolTip = "显示HelloWorld对话框";
            //b、设置图标
            Uri uri = new Uri(@"D:\Studay\CSharp\Work\RevitDeveloperGuide\_002AddPanel\img\world_32.ico");
            BitmapImage largeImage = new BitmapImage(uri);//需要添加引用using System.Windows.Media.Imaging;
            pushButton.LargeImage = largeImage;
            ribbonPanel.AddSeparator();

            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class HelloWorld : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            TaskDialog.Show("Revit", "Hello World!");

            return Result.Succeeded;
        }
    }
}