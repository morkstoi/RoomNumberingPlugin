using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumberingPlugin
{

    public class MainViewViewModel

    {
        private ExternalCommandData _commandData;
        public List<Level> Levels { get; } = new List<Level>();
        public List<View> LegendViews { get; } = new List<View>();
        public DelegateCommand WriteCommand { get; }
        public string Number { get; set; }
        public Level SelectedLevel { get; set; }
        public bool Checkbox { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //Вносим в CheckBox уровни из проекта
            Levels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>()
                .ToList();

            WriteCommand = new DelegateCommand(OnWriteCommand);

            Number = string.Empty;
        }

        private void OnWriteCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Number == null)
            {
                TaskDialog.Show("Ошибка", "Введите начальное значение");
                return;
            }

            foreach (var symbol in Number)
                if (!char.IsDigit(symbol))
                {
                    TaskDialog.Show("Ошибка", "Поле начального значения должно содержать только цифры");
                    return;
                }
            int Num = Convert.ToInt32(Number);

            if (!Checkbox && SelectedLevel == null)
            {
                TaskDialog.Show("Ошибка", "Выберите начальный уровень");
                return;
            }

            // Отбираем помещения в проекте
            List<Room> listAllRoom = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .ToList();
            List<Room> listRoom = new List<Room>();
            if (!Checkbox)
            {
                foreach (Room room in listAllRoom)
                {
                    Level level = room.Level;
                    string levelName = level.Name;
                    string SelectedLevelName = SelectedLevel.Name;
                    if (levelName == SelectedLevelName)
                    {
                        listRoom.Add(room);
                    }
                }
            }
            else
            {
                foreach (Room room in listAllRoom)
                {
                    listRoom.Add(room);
                }
            }
            // Прописываем нумерацию
            using (var ts = new Transaction(doc, "Write nums"))
            {
                ts.Start();
                foreach (Room room in listRoom)
                {
                    room.Number = Num.ToString();
                    Num++;
                }
                ts.Commit();
            }
            RaiseCloseRequest();
        }
        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
