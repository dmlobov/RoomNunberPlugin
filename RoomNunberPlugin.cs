using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNunberPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RoomNunberPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> levels = new FilteredElementCollector(doc)
                   .OfClass(typeof(Level))
                   .OfType<Level>()
                   .ToList();

            List<ElementId> rooms;

            Transaction ts1 = new Transaction(doc);
            ts1.Start("Создание помещений");
            foreach (Level l in levels)
            {
                rooms = (List<ElementId>)doc.Create.NewRooms2(l);
            }
            ts1.Commit();
           
            var filteredRooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms);
            
            List<ElementId> roomId = filteredRooms.ToElementIds() as List<ElementId>;
                       
            Transaction ts2 = new Transaction(doc);
            ts2.Start("Установка марок");
            foreach (ElementId r in roomId)
            {
                Element element = doc.GetElement(r);
                Room room = element as Room;
                string levelName = room.Level.Name.Substring(6);
                room.Name = $"{levelName}-этаж_{room.Number}-пом.";
                doc.Create.NewRoomTag(new LinkElementId(r), new UV(0, 0), null);
            }
            ts2.Commit();

            return Result.Succeeded;
        }
    }
}
