using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace _007TaskDialog {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            if (IsSuported(app))
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;
                TaskDialog mainDialog=new TaskDialog("Revit");
                mainDialog.MainInstruction = "Hello Revit!";
                mainDialog.MainContent =
                    "This sample shows how to use a Revit task dialog to communicate with the user.\n"
                    + "The command links below open additional task dialogs with more information.";
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,"查看Revit版本信息");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,"查看文档信息");
                mainDialog.CommonButtons = TaskDialogCommonButtons.Close;
                mainDialog.DefaultButton = TaskDialogResult.Close;
                mainDialog.FooterText = "<a href=\"https://www.5616760.com\">点击打开RevitAPI开发者中心</a>"; 
                TaskDialogResult tResuil = mainDialog.Show();
                if (TaskDialogResult.CommandLink1 == tResuil)
                {
                    TaskDialog dialog_Link1=new TaskDialog("Revit版本信息");
                    dialog_Link1.MainInstruction =
                        "Revit版本名：" + app.VersionName + "\nRevit版本号："+app.VersionNumber + "\nRevit编译版本：" + app.VersionBuild;
                    dialog_Link1.Show();
                }
                else if(TaskDialogResult.CommandLink2==tResuil)
                {
                    TaskDialog dialog_Link2=new TaskDialog("文档信息");
                    dialog_Link2.MainInstruction = "当前文档：" + doc.Title + "\n当前视图名：" + doc.ActiveView.Name;
                    dialog_Link2.Show();
                }
                return Result.Succeeded;
            }
            else
            {
                return Result.Failed;
            }
        }

        public bool IsSuported(Application app)
        {
            if (app.VersionNumber=="2020"&&String.Compare(app.VersionBuild,"20190201")>0)
            {
                return true;
            }
            else
            {
                TaskDialog dialog=new TaskDialog("不支持");
                dialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                dialog.MainInstruction="此应用不支持当前版本Revit，请使用Revit2020或更高版本！";
                dialog.Show();
                return false;
            }
        }
    }
}
