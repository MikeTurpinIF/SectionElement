#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
#endregion

namespace SectionElement
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {

            RibbonPanel panel = a.CreateRibbonPanel(Tab.AddIns, "Section Tool");


            //Create button data
            PushButtonData pbd1 = new PushButtonData("pb1", "Section\nElement", typeof(SectionElement).Assembly.Location, typeof(SectionElement).FullName);

            //Add buttons to panels
            var pb1 = panel.AddItem(pbd1) as PushButton;



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public static void AppEvent_View_Handler(object sender, EventArgs args)
        {
            UIApplication uiApplication = sender as UIApplication;
            Document document = uiApplication.ActiveUIDocument.Document;
            ViewActivatedEventArgs activatedEventArgs = args as ViewActivatedEventArgs;
            if (TaskDialog.Show("Element Section", "Do you want to delete the created section?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
            {
                using (Transaction transaction = new Transaction(document))
                {
                    transaction.Start("Delete Working Section");
                    document.Delete((activatedEventArgs.PreviousActiveView).Id);
                    transaction.Commit();
                }
            }
            uiApplication.ViewActivated -= new EventHandler<ViewActivatedEventArgs>(AppEvent_View_Handler);
        }
    }
}
