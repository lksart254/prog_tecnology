using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Exel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

class Program
{
    private static UserInterface ui = new UserInterface();

    private static bool processingMode = false;
    private static string filePath = "D:\\Program\\Development\\PROJECT\\C#\\test\\bin\\Debug\\net8.0\\data.xlsx";

    static void Main(string[] args)
    {
        ui.UserInput += OnUserInput;

        IList<Tank> tanks = null;
        IList<Unit> units = null;
        IList<Factory> factories = null;

        if (processingMode)
        {
            // Чтение данных из JSON файлов
            tanks = ReadJsonFile<IList<Tank>>("tanks.json");
            units = ReadJsonFile<IList<Unit>>("units.json");
            factories = ReadJsonFile<IList<Factory>>("factories.json");
        }
        else
        {
            // Чтение данных из EXEL файлов
            tanks = ReadTanksFromExcel(filePath);
            units = ReadUnitsFromExcel(filePath);
            factories = ReadFactoriesFromExcel(filePath);
        }

        /* Первый способ создания объектов
        var tanks = GetTanks();
        var units = GetUnits();
        var factories = GetFactories();
        */

        bool exit = false;
        while (!exit)
        {
            ui.WriteLine("\nМеню:");
            ui.WriteLine("1. Добавить");
            ui.WriteLine("2. Изменить");
            ui.WriteLine("3. Удалить");
            ui.WriteLine("4. Поиск резервуаров (с указанием принадлежности установке и заводу)");
            ui.WriteLine("5. Количество резервуаров и установок");
            ui.WriteLine("6. Общий объем резервуаров");
            ui.WriteLine("7. Список всех резервуаров");
            ui.WriteLine("0. Выход");

            string choice = ui.ReadLine("Введите номер операции: ");
            try
            {
                switch (choice)
                {
                    case "1":
                        AddItem(tanks, units, factories);
                        break;
                    case "2":
                        UpdateItem(tanks, units, factories);
                        break;
                    case "3":
                        DeleteItem(tanks, units, factories);
                        break;
                    case "4":
                        // Запрос ввода названия резервуара для поиска
                        ui.Write("\nВведите название резервуара для поиска: ");
                        string searchName = ui.ReadLine();
                        // Поиск резервуара по введенному названию
                        var foundTank = tanks.FirstOrDefault(t => t.Name.Contains(searchName));
                        if (foundTank != null)
                        {
                            // Поиск установки, которой принадлежит "Резервуар 2" (синтаксис запросов)
                            var foundUnit_QuerySyntax = FindUnit_QuerySyntax((IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Tank>)tanks, foundTank.Name);
                            // Поиск завода, которому принадлежит найденная установка (синтаксис запросов)
                            var foundFactory_QuerySyntax = FindFactory_QuerySyntax((IReadOnlyCollection<Factory>)factories, foundUnit_QuerySyntax);
                            // Вывод информации о принадлежности "Резервуара 2" установке и заводу (синтаксис запросов)
                            ui.WriteLine($"Найдено: {foundTank.Name}. Принадлежит установке {foundUnit_QuerySyntax.Name} и заводу {foundFactory_QuerySyntax.Name} (синтаксис запросов)");

                            // Поиск установки, которой принадлежит "Резервуар 2"(синтаксис методов)
                            var foundUnit_MethodSyntax = FindUnit_MethodSyntax((IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Tank>)tanks, foundTank.Name);
                            // Поиск завода, которому принадлежит найденная установка(синтаксис методов)
                            var foundFactory_MethodSyntax = FindFactory_MethodSyntax((IReadOnlyCollection<Factory>)factories, foundUnit_MethodSyntax);
                            // Вывод информации о принадлежности "Резервуара 2" установке и заводу(синтаксис методов)
                            ui.WriteLine($"Найдено: {foundTank.Name}. Принадлежит установке {foundUnit_MethodSyntax.Name} и заводу {foundFactory_MethodSyntax.Name} (синтаксис методов)\n");
                        }
                        else
                        {
                            // Вывод сообщения, если резервуар не найден
                            ui.WriteLine("Резервуар не найден");
                        }
                        break;
                    case "5":
                        ui.WriteLine($"Количество резервуаров: {tanks.Count}, " +
                            $"установок: {units.Count}");
                        break;
                    case "6":
                        // Получение общего объема всех резервуаров (синтаксис запросов)
                        var totalVolume_QuerySyntax = GetTotalVolume_QuerySyntax((IReadOnlyCollection<Tank>)tanks);
                        ui.WriteLine($"Общий объем резервуаров: {totalVolume_QuerySyntax} (синтаксис запросов)");
                        
                        // Получение общего объема всех резервуаров(синтаксис методов)
                        var totalVolume_MethodSyntax = GetTotalVolume_MethodSyntax((IReadOnlyCollection<Tank>)tanks);
                        ui.WriteLine($"Общий объем резервуаров: {totalVolume_MethodSyntax} (синтаксис методов)\n");
                        break;
                    case "7":
                        // Вывод информации о каждом резервуаре с указанием установки и завода
                        ui.WriteLine("Все резервуары:");
                        foreach (var tank in tanks)
                        {
                            var unit = units.First(u => u.Id == tank.UnitId);
                            var factory = factories.First(f => f.Id == unit.FactoryId);
                            ui.WriteLine($"ID: {tank.Id}, {tank.Name} ({unit.Name}, {factory.Name})");
                        }
                        break;
                    case "0":
                        exit = true;
                        // Сериализация всех объектов в JSON файл
                        SerializeToJson((IReadOnlyCollection<Tank>)tanks, (IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Factory>)factories);
                        break;
                    default:
                        ui.WriteLine("Неверный выбор. Попробуйте еще раз.");
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                ui.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (ArgumentNullException ex)
            {
                ui.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                ui.WriteLine($"Непредсказуемая ошибкка: {ex.Message}");
            }
        }
    }

    private static void OnUserInput(object sender, UserInputEventArgs e)
    {
        ui.WriteLine($"Пользователь ввел {e.Input} в {e.Timestamp:HH:mm:ss}");
    }

    // Метод для чтения данных из JSON файла
    public static T ReadJsonFile<T>(string fileName)
    {
        try
        {
            string jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (FileNotFoundException ex)
        {
            ui.WriteLine($"Файл {fileName} не найден. {ex.Message}");
            return default(T);
        }
        catch (JsonException ex)
        {
            ui.WriteLine($"Ошибка десериализации JSON из файла {fileName}. {ex.Message}");
            return default(T);
        }
        catch (FormatException ex)
        {
            ui.WriteLine($"Некорректный формат данных в файле {fileName}. {ex.Message}");
            return default(T);
        }
    }

    private static IList<Tank> ReadTanksFromExcel(string filePath)
    {
        var tanks = new List<Tank>();

        Exel.Application excelApp = new Exel.Application();
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        Exel.Worksheet worksheet = workbook.Sheets["Tanks"];
        Exel.Range range = worksheet.UsedRange;

        for (int row = 2; row <= range.Rows.Count; row++)
        {
            Tank tank = new Tank
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString(),
                Volume = Convert.ToInt32(((Exel.Range)range.Cells[row, 4]).Value2),
                MaxVolume = Convert.ToInt32(((Exel.Range)range.Cells[row, 5]).Value2),
                UnitId = Convert.ToInt32(((Exel.Range)range.Cells[row, 6]).Value2)
            };
            tanks.Add(tank);
        }

        workbook.Close();
        excelApp.Quit();
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        return tanks;
    }

    private static IList<Unit> ReadUnitsFromExcel(string filePath)
    {
        var units = new List<Unit>();

        Exel.Application excelApp = new Exel.Application();
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        Exel.Worksheet worksheet = workbook.Sheets["Units"];
        Exel.Range range = worksheet.UsedRange;

        for (int row = 2; row <= range.Rows.Count; row++)
        {
            Unit unit = new Unit
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString(),
                FactoryId = Convert.ToInt32(((Exel.Range)range.Cells[row, 4]).Value2)
            };
            units.Add(unit);
        }

        workbook.Close();
        excelApp.Quit();
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        return units;
    }

    private static IList<Factory> ReadFactoriesFromExcel(string filePath)
    {
        var factories = new List<Factory>();

        Exel.Application excelApp = new Exel.Application();
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        Exel.Worksheet worksheet = workbook.Sheets["Factories"];
        Exel.Range range = worksheet.UsedRange;

        for (int row = 2; row <= range.Rows.Count; row++)
        {
            Factory factory = new Factory
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString()
            };
            factories.Add(factory);
        }

        workbook.Close();
        excelApp.Quit();
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        return factories;
    }

    static void AddItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Добавление нового резервуара
        bool isValidTankId, isValidUnitId;
        int tankId, unitId;

        do
        {
            ui.Write("Введите ID резервуара, auto - для автоматического подбора или 0 для выхода: ");
            string input = ui.ReadLine();
            if (input == "0")
                return;

            if (input.ToLower() == "auto")
            {
                tankId = 1;
                while (true)
                {
                    if (!tanks.Any(t => t.Id == tankId))
                    {
                        break;
                    }
                    else
                    {
                        tankId++;
                    }
                }
                break;
            }

            isValidTankId = int.TryParse(input, out tankId) && !tanks.Any(t => t.Id == tankId);
            if (!isValidTankId)
                ui.WriteLine("Некорректный или занятый ID резервуара. Повторите ввод или введите 0 для отмены.");
        } while (!isValidTankId);


        ui.Write("Введите название резервуара: ");
        string tankName = ui.ReadLine();
        ui.Write("Введите описание резервуара: ");
        string tankDescription = ui.ReadLine();
        ui.Write("Введите объем резервуара: ");
        int tankVolume = int.Parse(ui.ReadLine());
        ui.Write("Введите максимальный объем резервуара: ");
        int tankMaxVolume = int.Parse(ui.ReadLine());


        do
        {
            ui.Write("Введите ID установки или 0 для отмены: ");
            string input = ui.ReadLine();
            if (input == "0")
                return;

            isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
            if (!isValidUnitId)
                ui.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
        } while (!isValidUnitId);

        Tank newTank = new Tank
        {
            Id = tankId,
            Name = tankName,
            Description = tankDescription,
            Volume = tankVolume,
            MaxVolume = tankMaxVolume,
            UnitId = unitId
        };

        tanks.Add(newTank);

        if (processingMode)
        {
            // При работе с JSON файлами
            SerializeToJsonFile("tanks.json", tanks);
        }
        else
        {
            // При работе с EXEL файлами
            WriteToExcel(filePath, tanks, units, factories);
        }

        ui.WriteLine("Резервуар успешно добавлен.");
    }

    static void UpdateItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Изменение существующего резервуара
        ui.Write("Введите ID резервуара для изменения: ");
        int tankId = int.Parse(ui.ReadLine());

        Tank tankToUpdate = tanks.FirstOrDefault(t => t.Id == tankId);

        if (tankToUpdate != null)
        {
            bool isValidUnitId;
            int unitId;

            ui.Write("Введите новое название резервуара: ");
            tankToUpdate.Name = ui.ReadLine();
            ui.Write("Введите новое описание резервуара: ");
            tankToUpdate.Description = ui.ReadLine();
            ui.Write("Введите новый объем резервуара: ");
            tankToUpdate.Volume = int.Parse(ui.ReadLine());
            ui.Write("Введите новый максимальный объем резервуара: ");
            tankToUpdate.MaxVolume = int.Parse(ui.ReadLine());

            do
            {
                ui.Write("Введите ID установки или 0 для отмены: ");
                string input = ui.ReadLine();
                if (input == "0")
                    return;

                isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
                if (!isValidUnitId)
                    ui.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
            } while (!isValidUnitId);

            if (processingMode)
            {
                // При работе с JSON файлами
                SerializeToJsonFile("tanks.json", tanks);
            }
            else
            {
                // При работе с EXEL файлами
                WriteToExcel(filePath, tanks, units, factories);
            }

            ui.WriteLine("Резервуар успешно изменен.");
        }
        else
        {
            ui.WriteLine("Резервуар с указанным ID не найден.");
        }
    }

    static void DeleteItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Удаление существующего резервуара
        ui.Write("Введите ID резервуара для удаления: ");
        int tankId = int.Parse(ui.ReadLine());

        Tank tankToDelete = tanks.FirstOrDefault(t => t.Id == tankId);

        if (tankToDelete != null)
        {
            tanks.Remove(tankToDelete);

            if (processingMode)
            {
                // При работе с JSON файлами
                SerializeToJsonFile("tanks.json", tanks);
            }
            else
            {
                // При работе с EXEL файлами
                WriteToExcel(filePath, tanks, units, factories);
            }

            ui.WriteLine("Резервуар успешно удален.");
        }
        else
        {
            ui.WriteLine("Резервуар с указанным ID не найден.");
        }
    }

    private static void WriteToExcel(string filePath, IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        Exel.Application excelApp = new Exel.Application();
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);

        // Запись данных резервуаров
        Exel.Worksheet tankSheet = workbook.Sheets["Tanks"];
        Exel.Range tankRange = tankSheet.UsedRange;
        tankRange.Clear();
        tankRange.Cells[1, 1] = "Id";
        tankRange.Cells[1, 2] = "Name";
        tankRange.Cells[1, 3] = "Description";
        tankRange.Cells[1, 4] = "Volume";
        tankRange.Cells[1, 5] = "MaxVolume";
        tankRange.Cells[1, 6] = "UnitId";

        for (int i = 0; i < tanks.Count; i++)
        {
            tankRange.Cells[i + 2, 1] = tanks[i].Id;
            tankRange.Cells[i + 2, 2] = tanks[i].Name;
            tankRange.Cells[i + 2, 3] = tanks[i].Description;
            tankRange.Cells[i + 2, 4] = tanks[i].Volume;
            tankRange.Cells[i + 2, 5] = tanks[i].MaxVolume;
            tankRange.Cells[i + 2, 6] = tanks[i].UnitId;
        }

        // Запись данных установок
        Exel.Worksheet unitSheet = workbook.Sheets["Units"];
        Exel.Range unitRange = unitSheet.UsedRange;
        unitRange.Clear();
        unitRange.Cells[1, 1] = "Id";
        unitRange.Cells[1, 2] = "Name";
        unitRange.Cells[1, 3] = "Description";
        unitRange.Cells[1, 4] = "FactoryId";

        for (int i = 0; i < units.Count; i++)
        {
            unitRange.Cells[i + 2, 1] = units[i].Id;
            unitRange.Cells[i + 2, 2] = units[i].Name;
            unitRange.Cells[i + 2, 3] = units[i].Description;
            unitRange.Cells[i + 2, 4] = units[i].FactoryId;
        }

        // Запись данных заводов
        Exel.Worksheet factorySheet = workbook.Sheets["Factories"];
        Exel.Range factoryRange = factorySheet.UsedRange;
        factoryRange.Clear();
        factoryRange.Cells[1, 1] = "Id";
        factoryRange.Cells[1, 2] = "Name";
        factoryRange.Cells[1, 3] = "Description";

        for (int i = 0; i < factories.Count; i++)
        {
            factoryRange.Cells[i + 2, 1] = factories[i].Id;
            factoryRange.Cells[i + 2, 2] = factories[i].Name;
            factoryRange.Cells[i + 2, 3] = factories[i].Description;
        }

        workbook.Save();
        workbook.Close();
        excelApp.Quit();
        Marshal.ReleaseComObject(factorySheet);
        Marshal.ReleaseComObject(unitSheet);
        Marshal.ReleaseComObject(tankSheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);
    }

    /* Первый способ создания объектов
    // Метод для получения массива резервуаров
    public static Tank[] GetTanks()
    {
        // Возвращает массив объектов Tank, созданных с помощью new
        return new Tank[]
        {
            new Tank { Id = 1, Name = "Резервуар 1", Description = "Надземный - вертикальный", Volume = 1500, MaxVolume = 2000, UnitId = 1 },
            new Tank { Id = 2, Name = "Резервуар 2", Description = "Надземный - горизонтальный", Volume = 2500, MaxVolume = 3000, UnitId = 1 },
            new Tank { Id = 3, Name = "Дополнительный резервуар 24", Description = "Надземный - горизонтальный", Volume = 3000, MaxVolume = 3000, UnitId = 2 },
            new Tank { Id = 4, Name = "Резервуар 35", Description = "Надземный - вертикальный", Volume = 3000, MaxVolume = 3000, UnitId = 2 },
            new Tank { Id = 5, Name = "Резервуар 47", Description = "Подземный - двустенный", Volume = 4000, MaxVolume = 5000, UnitId = 2 },
            new Tank { Id = 6, Name = "Резервуар 256", Description = "Подводный", Volume = 500, MaxVolume = 500, UnitId = 3 }
        };
    }

    // Метод для получения массива установок
    public static Unit[] GetUnits()
    {
        // Возвращает массив объектов Unit, созданных с помощью new
        return new Unit[]
        {
            new Unit { Id = 1, Name = "ГФУ-2", Description = "Газофракционирующая установка", FactoryId = 1 },
            new Unit { Id = 2, Name = "ABT-6", Description = "Атмосферно-вакуумная трубчатка", FactoryId = 1 },
            new Unit { Id = 3, Name = "ABT-10", Description = "Атмосферно-вакуумная трубчатка", FactoryId = 2 }
        };
    }

    // Метод для получения массива заводов
    public static Factory[] GetFactories()
    {
        // Возвращает массив объектов Factory, созданных с помощью new
        return new Factory[]
        {
            new Factory { Id = 1, Name = "НПЗ#1", Description = "Первый нефтеперерабатывающий завод" },
            new Factory { Id = 2, Name = "НПЗ#2", Description = "Второй нефтеперерабатывающий завод" }
        };
    }
    */

    // Метод для поиска установки по имени резервуара (синтаксис методов)
    public static Unit FindUnit_MethodSyntax(IReadOnlyCollection<Unit> units, IReadOnlyCollection<Tank> tanks, string tankName)
    {
        if (units == null || tanks == null || string.IsNullOrEmpty(tankName))
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        var foundUnit = units.FirstOrDefault(u => tanks.Any(t => t.Name == tankName && t.UnitId == u.Id));
        if (foundUnit == null)
            throw new InvalidOperationException($"Установка для резервуара '{tankName}' не найдена.");

        return foundUnit;
    }

    // Метод для поиска завода по установке (синтаксис методов)
    public static Factory FindFactory_MethodSyntax(IReadOnlyCollection<Factory> factories, IIdentifiable unit)
    {
        if (factories == null || unit == null)
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        var foundFactory = factories.FirstOrDefault(f => f.Id == unit.Id);
        if (foundFactory == null)
            throw new InvalidOperationException($"Завод для установки '{unit.Name}' не найден.");

        return foundFactory;
    }

    // Метод для получения общего объема всех резервуаров (синтаксис методов)
    public static int GetTotalVolume_MethodSyntax(IReadOnlyCollection<Tank> tanks)
    {
        if (tanks == null)
            throw new ArgumentNullException("Параметр 'tanks' имеет значение null.");

        var totalVolume = tanks.Sum(t => t.Volume);
        if (totalVolume == 0)
            throw new InvalidOperationException("Общий объем резервуаров не может быть вычислен. Нет резервуаров.");

        return totalVolume;
    }

    // Метод для поиска установки по имени резервуара (синтаксис запросов)
    public static Unit FindUnit_QuerySyntax(IReadOnlyCollection<Unit> units, IReadOnlyCollection<Tank> tanks, string tankName)
    {
        if (units == null || tanks == null || string.IsNullOrEmpty(tankName))
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        var query = from unit in units
                    join tank in tanks on unit.Id equals tank.UnitId
                    where tank.Name == tankName
                    select unit;

        var foundUnit = query.FirstOrDefault();
        if (foundUnit == null)
            throw new InvalidOperationException($"Установка для резервуара '{tankName}' не найдена.");

        return foundUnit;
    }

    // Метод для поиска завода по установке (синтаксис запросов)
    public static Factory FindFactory_QuerySyntax(IReadOnlyCollection<Factory> factories, IIdentifiable unit)
    {
        if (factories == null || unit == null)
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        var query = from factory in factories
                    where factory.Id == unit.Id
                    select factory;

        var foundFactory = query.FirstOrDefault();
        if (foundFactory == null)
            throw new InvalidOperationException($"Завод для установки '{unit.Name}' не найден.");

        return foundFactory;
    }

    // Метод для получения общего объема всех резервуаров (синтаксис запросов)
    public static int GetTotalVolume_QuerySyntax(IReadOnlyCollection<Tank> tanks)
    {
        if (tanks == null)
            throw new ArgumentNullException("Параметр 'tanks' имеют значение null.");

        var query = from tank in tanks
                    select tank.Volume;

        var totalVolume = query.Sum();
        if (totalVolume == 0)
            throw new InvalidOperationException("Общий объем резервуаров не может быть вычислен. Нет резервуаров.");

        return totalVolume;
    }

    // Метод для сериализации всех объектов в JSON файл
    public static void SerializeToJson(IReadOnlyCollection<Tank> tanks, IReadOnlyCollection<Unit> units, IReadOnlyCollection<Factory> factories)
    {
        var data = new
        {
            Tanks = tanks,
            Units = units,
            Factories = factories
        };

        try
        {
            string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("data.json", jsonString);
        }
        catch (ArgumentException ex)
        {
            ui.WriteLine($"Ошибка сериализации данных в JSON. {ex.Message}");
        }
    }

    static void SerializeToJsonFile<T>(string fileName, T data)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, jsonString);
        }
        catch (Exception ex)
        {
            ui.WriteLine($"Ошибка сериализации данных в JSON файл {fileName}. {ex.Message}");
        }
    }
}

public class UserInterface
{
    public event EventHandler<UserInputEventArgs> UserInput;

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void Write(string message)
    {
        Console.Write(message);
    }

    public string ReadLine(string prompt)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();

        if (int.TryParse(input, out int value) || double.TryParse(input, out double d_value))
            OnUserInput(input);

        return input;
    }

    public string ReadLine()
    {
        string input = Console.ReadLine();

        if (int.TryParse(input, out int value))
            OnUserInput(input);

        return input;
    }

    protected virtual void OnUserInput(string input)
    {
        UserInput?.Invoke(this, new UserInputEventArgs(input, DateTime.Now));
    }
}

public class UserInputEventArgs : EventArgs
{
    public string Input { get; }
    public DateTime Timestamp { get; }

    public UserInputEventArgs(string input, DateTime timestamp)
    {
        Input = input;
        Timestamp = timestamp;
    }
}

// Интерфейс IIdentifiable, объединяющий классы Factory, Unit, Tank
public interface IIdentifiable
{
    int Id { get; set; }
    string Name { get; set; }
}

// Класс Unit, представляющий установку, реализует интерфейс IIdentifiable
public class Unit : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int FactoryId { get; set; }
}

// Класс Factory, представляющий завод, реализует интерфейс IIdentifiable
public class Factory : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

// Класс Tank, представляющий резервуар, реализует интерфейс IIdentifiable
public class Tank : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Volume { get; set; }
    public int MaxVolume { get; set; }
    public int UnitId { get; set; }
}
