using Newtonsoft.Json;
using ScottPlot;
using ServiceStack;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TestForTexode.Models;

namespace TestForTexode
{
    public partial class MainWindow : Window
    {
        private TableData SelectedItem { get; set; }
        private static string FolderPath;

        public MainWindow()
        {
            // Берем файл по поторому нам надо создать таблицу
            GetFolderPath();
            InitializeComponent();
            Graph.Plot.XLabel("Дни");
            Graph.Plot.YLabel("Шаги");
            DataTable.ItemsSource = GetDataToTable();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружам таблицу и помечаем красным цветом тех пользователей чьи лучшие или худшие результаты
            // отличаются от среднего количества шагов за весь период (по этому пользователю) более чем на 20%.
            DrawRowsWith20PercentDifference();
            DataTable.SelectionChanged += DataTable_SelectionChanged;
        }

        private void DataTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = (TableData)DataTable.SelectedItem;
            //Риссуем график и выделяем максимум и минимум
            HighlightMinMaxValueAndWrite();
        }

        // Метод который берет путь с папкой в которой лежат нашы Json файлы с даннми о пользователях
        private void GetFolderPath()
        {
            var ds = new MyDialogService();
            bool success = false;
            while (success == false)
            {
                if (ds.OpenFolderDialog() == false)
                {
                    MessageBox.Show("Все таки нужно было выбрать папку!");
                    Environment.Exit(0);
                }
                else
                {
                    var files = Directory.GetFiles(ds.FilePath);
                    if (files != null && files.Any(x => x.EndsWith(".json")))
                        success = true;
                    else
                        MessageBox.Show("а может Json?)");

                }
            }
            FolderPath = ds.FilePath;
        }

        // Выносим данные о пользовате со всеми его днями чтобы не метод MakeOneUsersWithAllHisDeys() не работал много раз 
        private static List<UserWithAllHisData> _users = new();
        public static List<UserWithAllHisData> Users
        {
            get
            {
                if (_users.Count == 0)
                    _users = MakeOneUsersWithAllHisDeys();
                return _users;
            }
        }

        // Метод который берет все данные из Json файлов в один массив
        private static IEnumerable<BaseJsonData> GetAllDataFromJsons()
        {
            int count = 1;
            foreach (var file in Directory.GetFiles(FolderPath))
            {
                string data = File.ReadAllText(file);
                var result = JsonConvert.DeserializeObject<BaseJsonData[]>(data);
                if (result == null)
                {
                    MessageBox.Show($"Ошибка чтения'{file}'");
                    break;
                }

                foreach (var item in result)
                {
                    item.Day = count;
                    yield return item;
                }
                count++;
            }
        }

        // Метод который собирает все данные об одной пользователе по имени в один массив
        private static List<UserWithAllHisData> MakeOneUsersWithAllHisDeys()
        {
            List<UserWithAllHisData> Users = new();
            foreach (var currentUser in GetAllDataFromJsons())
                if (!Users.Any(x => x.Name == currentUser.User))
                {
                    var user = new UserWithAllHisData(currentUser.User);
                    user.Steps.Add(new DaySteps(currentUser.Rank, currentUser.Day, currentUser.Steps, currentUser.Status));
                    Users.Add(user);
                }
                else
                {
                    var user = Users.FirstOrDefault(x => x.Name == currentUser.User);
                    user.Steps.Add(new DaySteps(currentUser.Rank, currentUser.Day, currentUser.Steps, currentUser.Status));
                }
            return Users;
        }

        // Метод который создает данные для таблицы
        private static List<TableData> GetDataToTable()
        {
            List<TableData> UsersToWriteOnOnTable = new();
            foreach (var data in Users)
                UsersToWriteOnOnTable.Add(new TableData(data.Name, Convert.ToInt32(data.Steps.Average(x => x.Steps)), Convert.ToInt32(data.Steps.Max(x => x.Steps)), Convert.ToInt32(data.Steps.Min(x => x.Steps))));

            return UsersToWriteOnOnTable;
        }

        // Событие которое сохраняет данные о выбранном объекте таблицы
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var DialogService = new MyDialogService();
            if (DataTable.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите человека данные о котором вы хотите сахранить");
                return;
            }

            var ToFile = new
            {
                SelectedItem.Name,
                SelectedItem.StepsMax,
                SelectedItem.StepsMin,
                SelectedItem.Average,
                Users.First(x => x.Name == SelectedItem.Name).Steps
            };
            switch (FormatIdex.Text)
            {
                case "Json":
                    try
                    {
                        if (DialogService.SaveFileDialog(SelectedItem.Name, ".json"))
                            using (StreamWriter writer = File.CreateText(DialogService.FilePath))
                            {
                                string output = JsonConvert.SerializeObject(ToFile);
                                writer.Write(output);
                                MessageBox.Show($"Сохронене Json файла с именем '{SelectedItem.Name}' произведено успешно");
                            }
                    }
                    catch (IOException exc)
                    {
                        MessageBox.Show($"Возника ошибка при сохранинии: {exc.Message} в Json формате");
                    }

                    break;
                case "XML":
                    try
                    {
                        if (DialogService.SaveFileDialog(SelectedItem.Name, ".xml"))
                            using (StreamWriter writer = File.CreateText(DialogService.FilePath))
                            {
                                string output = XmlSerializer.SerializeToString(ToFile);
                                writer.Write(output);
                                MessageBox.Show($"Сохронене XML файла с именем '{SelectedItem.Name}' произведено успешно");
                            }
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show($"Возника ошибка при сохраниние: {exc.Message} в XML формате");
                    }

                    break;
                case "CSV":
                    try
                    {
                        if (DialogService.SaveFileDialog(SelectedItem.Name, ".csv"))
                            using (StreamWriter writer = File.CreateText(DialogService.FilePath))
                            {
                                var lList = new List<object> { ToFile };
                                string output = CsvSerializer.SerializeToCsv(lList);
                                writer.Write(output);
                                MessageBox.Show($"Сохронене CSV файла с именем '{SelectedItem.Name}' произведено успешно");
                            }
                    }
                    catch (IOException exc)
                    {
                        MessageBox.Show($"Возника ошибка при сохраниние: {exc.Message} в CSV формате");
                    }

                    break;
            }
        }
        //Метод который раскрашивает тех пользователей у которых максимальное или минимальное
        //значение отличается от среднего на 20%
        private void DrawRowsWith20PercentDifference()
        {
            foreach (TableData datauser in DataTable.ItemsSource)
            {
                if (((float)datauser.StepsMax / (float)datauser.Average - 1) * 100 >= 20 || (1 - (float)datauser.StepsMin / (float)datauser.Average) * 100 >= 20)
                    if (DataTable.ItemContainerGenerator.ContainerFromItem(datauser) is DataGridRow row)
                        row.Background = Brushes.LightCoral;
            }
        }

        //Метод который указывает на минимальные и максимальные значения на графике
        private void HighlightMinMaxValueAndWrite()
        {
            var user = Users.First(x => x.Name == SelectedItem.Name);
            Graph.Plot.Clear();
            Graph.Plot.AddScatter(user.Steps.Select(x => Convert.ToDouble(x.Day)).ToArray(), user.Steps.Select(x => Convert.ToDouble(x.Steps)).ToArray());
            var myDraggableMaxMarker = new ScottPlot.Plottable.DraggableMarkerPlot()
            {
                X = user.Steps.First(x => x.Steps == SelectedItem.StepsMax).Day,
                Y = SelectedItem.StepsMax,
                Color = System.Drawing.Color.IndianRed,
                MarkerShape = MarkerShape.filledDiamond,
                MarkerSize = 15,
                Text = "Максимум шагов",
            };
            Graph.Plot.Add(myDraggableMaxMarker);
            var xMin = user.Steps.First(x => x.Steps == SelectedItem.StepsMin).Day;
            var myDraggableMinMarker = new ScottPlot.Plottable.DraggableMarkerPlot()
            {
                X = user.Steps.First(x => x.Steps == SelectedItem.StepsMin).Day,
                Y = SelectedItem.StepsMin,
                Color = System.Drawing.Color.CadetBlue,
                MarkerShape = MarkerShape.filledDiamond,
                MarkerSize = 15,
                Text = "Минимум шагов",
            };
            Graph.Plot.Add(myDraggableMinMarker);
            Graph.Refresh();
        }

        private void DataTable_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
