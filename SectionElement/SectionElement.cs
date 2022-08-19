using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SectionElement
{
    [Transaction(TransactionMode.Manual)]
    public class SectionElement : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication application = commandData.Application;
            UIDocument activeUiDocument = application.ActiveUIDocument;
            Document document = activeUiDocument.Document;
            Line line = null;
            Selection selection = activeUiDocument.Selection;
            ElementId elementId;
            if (selection.GetElementIds().Count == 1)
            {
                elementId = ((IEnumerable<ElementId>)selection.GetElementIds()).First();
            }
            else
            {
                try
                {
                    elementId = selection.PickObject(ObjectType.Element, "Please select an element to section").ElementId;
                }
                catch
                {
                    return Result.Cancelled;
                }
            }
            Element element = document.GetElement(elementId);
            if (element == null)
            {
                message = "No element selected";
                return Result.Cancelled;
            }
            if (element.Location != null && element.Location is LocationPoint)
            {
                XYZ point = (element.Location as LocationPoint).Point;
                XYZ xyz1 = point.Subtract(new XYZ(0.0, 0.0, point.Z));
                XYZ xyz2 = (element as FamilyInstance).FacingOrientation.CrossProduct(XYZ.BasisZ);
                double num = document.GetElement(element.GetTypeId()).LookupParameter("Width").AsDouble();
                line = Line.CreateBound(xyz1.Subtract(xyz2.Multiply(num / 2.0)), xyz1.Add(xyz2.Multiply(num / 2.0)));
            }
            else if (element.Location != null && element.Location is LocationCurve)
                line = (element.Location as LocationCurve).Curve as Line;
            if (line == null)
            {
                message = "Unable to retrieve element location.";
                return Result.Failed;
            }
            XYZ endPoint = line.GetEndPoint(0);
            XYZ xyz3 = line.GetEndPoint(1).Subtract(endPoint);
            ElementId defaultElementTypeId = document.GetDefaultElementTypeId(ElementTypeGroup.ViewTypeSection);
            BoundingBoxXYZ boundingBoxXyz1 = element.get_BoundingBox(null);
            double z1 = boundingBoxXyz1.Min.Z;
            double z2 = boundingBoxXyz1.Max.Z;
            double num1 = xyz3.GetLength() / 2.0;
            double num2 = 0.09 * num1;
            double num3 = 0.1 * num1;
            double num4 = 0.6 * num1;
            XYZ xyz4 = new XYZ(-num1 - num2, z1 - num3, -num4);
            XYZ xyz5 = new XYZ(num1 + num2, z2 + num3, num4);
            XYZ xyz6 = endPoint.Add(xyz3.Multiply(0.5));
            XYZ xyz7 = xyz3.Normalize();
            XYZ basisZ = XYZ.BasisZ;
            XYZ xyz8 = xyz7.CrossProduct(basisZ);
            Transform identity = Transform.Identity;
            identity.Origin = xyz6;
            identity.BasisX = xyz7;
            identity.BasisY = basisZ;
            identity.BasisZ = xyz8;
            BoundingBoxXYZ boundingBoxXyz2 = new BoundingBoxXYZ();
            boundingBoxXyz2.Transform = identity;
            boundingBoxXyz2.Min = xyz4;
            boundingBoxXyz2.Max = xyz5;
            ViewSection section;
            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("Create Section View");
                section = ViewSection.CreateSection(document, defaultElementTypeId, boundingBoxXyz2);
                transaction.Commit();
            }
            activeUiDocument.ActiveView = section;
            application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(App.AppEvent_View_Handler);
            return Result.Succeeded;
        }
    }
}