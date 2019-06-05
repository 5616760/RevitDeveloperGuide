using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace _006IExternalApp {
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]//是否启用日志
    public class Class1 : IExternalApplication {
        //实现OnStartup方法以在revit启动时注册事件。
        public Result OnShutdown(UIControlledApplication application) {
            application.DialogBoxShowing-=new EventHandler<DialogBoxShowingEventArgs>(AppDialogShowing);
            return Result.Succeeded;
        }
        //实现此方法以在revit退出时注销订阅的事件。
        public Result OnStartup(UIControlledApplication application) {
            application.DialogBoxShowing+=new EventHandler<DialogBoxShowingEventArgs>(AppDialogShowing);
            return Result.Succeeded;
        }
        //显示事件处理程序的对话框，它允许您在对话框显示之前做一些工作
        private void AppDialogShowing(object sender, DialogBoxShowingEventArgs e)
        {
            string dialogId = e.DialogId;
            if(dialogId=="") return;
            TaskDialog taskDialog=new TaskDialog("Revit");
            taskDialog.MainContent = "A Revit dialog is about to be opened.\n" +
                                     "The DialogId of this dialog is " + dialogId + "\n" +
                                     "Press 'Cancel' to immediately dismiss the dialog";
            taskDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
            TaskDialogResult result = taskDialog.Show();
            if (result == TaskDialogResult.Cancel)
            {
                e.OverrideResult(1);
            }
        }
    }
}

