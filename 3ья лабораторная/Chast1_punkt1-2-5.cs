using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        // Чтение данных из JSON файлов
        var tanks = ReadJsonFile<Tank[]>("tanks.json");
        var units = ReadJsonFile<Unit[]>("units.json");
        var factories = ReadJsonFile<Factory[]>("factories.json");

        /* Первый способ создания объектов
        var tanks = GetTanks();
        var units = GetUnits();
        var factories = GetFactories();
        */

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Добавить");
            Console.WriteLine("2. Изменить");
            Console.WriteLine("3. Удалить");
            Console.WriteLine("4. Поиск резервуаров (с указанием принадлежности установке и заводу)");
            Console.WriteLine("5. Количество резервуаров и установок");
            Console.WriteLine("6. Общий объем резервуаров");
            Console.WriteLine("7. Список всех резервуаров");
            Console.WriteLine("0. Выход");

            Console.Write("Введите номер операции: ");
            string choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        AddItem(ref tanks, ref units, ref factories);
                        break;
                    case "2":
                        UpdateItem(ref tanks, ref units, ref factories);
                        break;
                    case "3":
                        DeleteItem(ref tanks, ref units, ref factories);
                        break;
                    case "4":
                        // Запрос ввода названия резервуара для поиска
                        Console.Write("\nВведите название резервуара для поиска: ");
                        string searchName = Console.ReadLine();
                        // Поиск резервуара по введенному названию
                        var foundTank = tanks.FirstOrDefault(t => t.Name.Contains(searchName));
                        if (foundTank != null)
                        {
                            // Поиск установки, которой принадлежит "Резервуар 2" (синтаксис запросов)
                            var foundUnit_QuerySyntax = FindUnit_QuerySyntax(units, tanks, foundTank.Name);
                            // Поиск завода, которому принадлежит найденная установка (синтаксис запросов)
                            var foundFactory_QuerySyntax = FindFactory_QuerySyntax(factories, foundUnit_QuerySyntax);
                            // Вывод информации о принадлежности "Резервуара 2" установке и заводу (синтаксис запросов)
                            Console.WriteLine($"Найдено: {foundTank.Name} принадлежит установке {foundUnit_QuerySyntax.Name} и заводу {foundFactory_QuerySyntax.Name} (синтаксис запросов)");
                            /* Тот же поиск, но синтаксисом методов
                            // Поиск установки, которой принадлежит "Резервуар 2" (синтаксис методов)
                            // var foundUnit_MethodSyntax = FindUnit_MethodSyntax(units, tanks, "Резервуар 2");
                            // Поиск завода, которому принадлежит найденная установка (синтаксис методов)
                            // var foundFactory_MethodSyntax = FindFactory_MethodSyntax(factories, foundUnit_MethodSyntax);
                            // Вывод информации о принадлежности "Резервуара 2" установке и заводу (синтаксис методов)
                            // Console.WriteLine($"Резервуар 2 принадлежит установке {foundUnit_MethodSyntax.Name} и заводу {foundFactory_MethodSyntax.Name} (синтаксис методов)\n");
                            */
                        }
                        else
                        {
                            // Вывод сообщения, если резервуар не найден
                            Console.WriteLine("Резервуар не найден");
                        }
                        break;
                    case "5":
                        // Вывод количества резервуаров и установок
                        Console.WriteLine($"Количество резервуаров: {(tanks != null ? tanks.Length : 0)}, " +
                            $"установок: {(units != null ? units.Length : 0)} \n");
                        break;
                    case "6":
                        // Получение общего объема всех резервуаров (синтаксис запросов)
                        var totalVolume_QuerySyntax = GetTotalVolume_QuerySyntax(tanks);
                        Console.WriteLine($"Общий объем резервуаров: {totalVolume_QuerySyntax} (синтаксис запросов)");
                        /* Получение общего объема всех резервуаров (синтаксис методов)
                        var totalVolume_MethodSyntax = GetTotalVolume_MethodSyntax(tanks);
                        Console.WriteLine($"Общий объем резервуаров: {totalVolume_MethodSyntax} (синтаксис методов)\n");
                        */
                        break;
                    case "7":
                        // Вывод информации о каждом резервуаре с указанием установки и завода
                        Console.WriteLine("Все резервуары:");
                        foreach (var tank in tanks)
                        {
                            var unit = units.First(u => u.Id == tank.UnitId);
                            var factory = factories.First(f => f.Id == unit.FactoryId);
                            Console.WriteLine($"{tank.Name} ({unit.Name}, {factory.Name})");
                        }
                        break;
                    case "0":
                        exit = true;
                        // Сериализация всех объектов в JSON файл
                        SerializeToJson(tanks, units, factories);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте еще раз.");
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Непредсказуемая ошибкка: {ex.Message}");
            }
        }
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
            Console.WriteLine($"Файл {fileName} не найден. {ex.Message}");
            return default(T);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка десериализации JSON из файла {fileName}. {ex.Message}");
            return default(T);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Некорректный формат данных в файле {fileName}. {ex.Message}");
            return default(T);
        }
    }

    static void AddItem(ref Tank[] tanks, ref Unit[] units, ref Factory[] factories)
    {
        // Добавление нового резервуара
        bool isValidTankId, isValidUnitId;
        int tankId, unitId;

        do
        {
            Console.Write("Введите ID резервуара, auto - для автоматического подбора или 0 для выхода: ");
            string input = Console.ReadLine();
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
                Console.WriteLine("Некорректный или занятый ID резервуара. Повторите ввод или введите 0 для отмены.");
        } while (!isValidTankId);


        Console.Write("Введите название резервуара: ");
        string tankName = Console.ReadLine();
        Console.Write("Введите описание резервуара: ");
        string tankDescription = Console.ReadLine();
        Console.Write("Введите объем резервуара: ");
        int tankVolume = int.Parse(Console.ReadLine());
        Console.Write("Введите максимальный объем резервуара: ");
        int tankMaxVolume = int.Parse(Console.ReadLine());


        do
        {
            Console.Write("Введите ID установки или 0 для отмены: ");
            string input = Console.ReadLine();
            if (input == "0")
                return;

            isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
            if (!isValidUnitId)
                Console.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
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

        tanks = tanks.Append(newTank).ToArray();
        SerializeToJsonFile("tanks.json", tanks);
        Console.WriteLine("Резервуар успешно добавлен.");
    }

    static void UpdateItem(ref Tank[] tanks, ref Unit[] units, ref Factory[] factories)
    {
        // Изменение существующего резервуара
        Console.Write("Введите ID резервуара для изменения: ");
        int tankId = int.Parse(Console.ReadLine());

        Tank tankToUpdate = tanks.FirstOrDefault(t => t.Id == tankId);

        if (tankToUpdate != null)
        {
            bool isValidUnitId;
            int unitId;

            Console.Write("Введите новое название резервуара: ");
            tankToUpdate.Name = Console.ReadLine();
            Console.Write("Введите новое описание резервуара: ");
            tankToUpdate.Description = Console.ReadLine();
            Console.Write("Введите новый объем резервуара: ");
            tankToUpdate.Volume = int.Parse(Console.ReadLine());
            Console.Write("Введите новый максимальный объем резервуара: ");
            tankToUpdate.MaxVolume = int.Parse(Console.ReadLine());

            do
            {
                Console.Write("Введите ID установки или 0 для отмены: ");
                string input = Console.ReadLine();
                if (input == "0")
                    return;

                isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
                if (!isValidUnitId)
                    Console.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
            } while (!isValidUnitId);

            SerializeToJsonFile("tanks.json", tanks);
            Console.WriteLine("Резервуар успешно изменен.");
        }
        else
        {
            Console.WriteLine("Резервуар с указанным ID не найден.");
        }
    }

    static void DeleteItem(ref Tank[] tanks, ref Unit[] units, ref Factory[] factories)
    {
        // Удаление существующего резервуара
        Console.Write("Введите ID резервуара для удаления: ");
        int tankId = int.Parse(Console.ReadLine());

        Tank tankToDelete = tanks.FirstOrDefault(t => t.Id == tankId);

        if (tankToDelete != null)
        {
            tanks = tanks.Where(t => t.Id != tankId).ToArray();
            SerializeToJsonFile("tanks.json", tanks);
            Console.WriteLine("Резервуар успешно удален.");
        }
        else
        {
            Console.WriteLine("Резервуар с указанным ID не найден.");
        }
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

    /* Функции с синтаксисом методов
    // Метод для поиска установки по имени резервуара (синтаксис методов)
    public static Unit FindUnit_MethodSyntax(Unit[] units, Tank[] tanks, string tankName)
    {
        return units.FirstOrDefault(u => tanks.Any(t => t.Name == tankName && t.UnitId == u.Id));
    }

    // Метод для поиска завода по установке (синтаксис методов)
    public static Factory FindFactory_MethodSyntax(Factory[] factories, Unit unit)
    {
        return factories.FirstOrDefault(f => f.Id == unit.FactoryId);
    }

    // Метод для получения общего объема всех резервуаров (синтаксис методов)
    public static int GetTotalVolume_MethodSyntax(Tank[] tanks)
    {
        return tanks.Sum(t => t.Volume);
    }
    */

    // Метод для поиска установки по имени резервуара (синтаксис запросов)
    public static Unit FindUnit_QuerySyntax(Unit[] units, Tank[] tanks, string tankName)
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
    public static Factory FindFactory_QuerySyntax(Factory[] factories, Unit unit)
    {
        if (factories == null || unit == null)
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        var query = from factory in factories
                    where factory.Id == unit.FactoryId
                    select factory;

        var foundFactory = query.FirstOrDefault();
        if (foundFactory == null)
            throw new InvalidOperationException($"Завод для установки '{unit.Name}' не найден.");

        return foundFactory;
    }

    // Метод для получения общего объема всех резервуаров (синтаксис запросов)
    public static int GetTotalVolume_QuerySyntax(Tank[] tanks)
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
    public static void SerializeToJson(Tank[] tanks, Unit[] units, Factory[] factories)
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
            Console.WriteLine($"Ошибка сериализации данных в JSON. {ex.Message}");
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
            Console.WriteLine($"Ошибка сериализации данных в JSON файл {fileName}. {ex.Message}");
        }
    }

}

// Класс Unit, представляющий установку
public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int FactoryId { get; set; }
}

// Класс Factory, представляющий завод
public class Factory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

// Класс Tank, представляющий резервуар
public class Tank
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Volume { get; set; }
    public int MaxVolume { get; set; }
    public int UnitId { get; set; }
}
