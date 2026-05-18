using System.Text;

// ══════════════════════════════════════════════════════════
// Точка входа — консольное меню
// ══════════════════════════════════════════════════════════
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "greenhouse.db";
string greenhouseCsv = Path.Combine(AppContext.BaseDirectory, "greenhouse.csv");
string plantCsv = Path.Combine(AppContext.BaseDirectory, "plant.csv");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(greenhouseCsv, plantCsv);

Console.WriteLine();

string choice;
do
{
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║       УПРАВЛЕНИЕ РАСТЕНИЯМИ          ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║  1 — Показать все теплицы            ║");
    Console.WriteLine("║  2 — Показать все растения           ║");
    Console.WriteLine("║  3 — Добавить растение               ║");
    Console.WriteLine("║  4 — Редактировать растение          ║");
    Console.WriteLine("║  5 — Удалить растение                ║");
    Console.WriteLine("║  6 — Отчёты                          ║");
    Console.WriteLine("║  7 — Фильтр по теплице               ║");
    Console.WriteLine("║  0 — Выход                           ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowGreenhouses(db); break;
        case "2": ShowPlants(db); break;
        case "3": AddPlant(db); break;
        case "4": EditPlant(db); break;
        case "5": DeletePlant(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByGreenhouse(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }

    Console.WriteLine();
}
while (choice != "0");

// ══════════════════════════════════════════════════════════
// Функции пунктов меню
// ══════════════════════════════════════════════════════════

static void ShowGreenhouses(DatabaseManager db)
{
    Console.WriteLine("--- Все теплицы ---");
    var list = db.GetAllGreenhouses();
    foreach (var g in list)
        Console.WriteLine("  " + g);
    Console.WriteLine($"Итого: {list.Count}");
}

static void ShowPlants(DatabaseManager db)
{
    Console.WriteLine("--- Все растения ---");
    var list = db.GetAllPlants();
    foreach (var p in list)
        Console.WriteLine("  " + p);
    Console.WriteLine($"Итого: {list.Count}");
}

static void AddPlant(DatabaseManager db)
{
    Console.WriteLine("--- Добавление растения ---");

    Console.WriteLine("Доступные теплицы:");
    var greenhouses = db.GetAllGreenhouses();
    foreach (var g in greenhouses)
        Console.WriteLine("  " + g);

    Console.Write("ID теплицы: ");
    if (!int.TryParse(Console.ReadLine(), out int ghId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    Console.Write("Название растения: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0)
    {
        Console.WriteLine("Ошибка: название не может быть пустым.");
        return;
    }

    Console.Write("Высота (см): ");
    if (!double.TryParse(Console.ReadLine(),
        System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture,
        out double height))
    {
        Console.WriteLine("Ошибка: введите число.");
        return;
    }

    try
    {
        var plant = new Plant(0, ghId, name, height);
        db.AddPlant(plant);
        Console.WriteLine("Растение добавлено.");
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

static void EditPlant(DatabaseManager db)
{
    Console.WriteLine("--- Редактирование растения ---");
    Console.Write("Введите ID растения: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var plant = db.GetPlantById(id);
    if (plant == null)
    {
        Console.WriteLine($"Растение с ID={id} не найдено.");
        return;
    }

    Console.WriteLine($"Текущие данные: {plant}");
    Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

    Console.Write($"Название [{plant.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0) plant.Name = input;

    Console.Write($"ID теплицы [{plant.GreenhouseId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newGhId))
        plant.GreenhouseId = newGhId;

    Console.Write($"Высота (см) [{plant.HeightCm}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && double.TryParse(input,
        System.Globalization.NumberStyles.Any,
        System.Globalization.CultureInfo.InvariantCulture,
        out double newHeight))
    {
        try
        {
            plant.HeightCm = newHeight;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return;
        }
    }

    db.UpdatePlant(plant);
    Console.WriteLine("Данные обновлены.");
}

static void DeletePlant(DatabaseManager db)
{
    Console.WriteLine("--- Удаление растения ---");
    Console.Write("Введите ID растения: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var plant = db.GetPlantById(id);
    if (plant == null)
    {
        Console.WriteLine($"Растение с ID={id} не найдено.");
        return;
    }

    Console.Write($"Удалить «{plant.Name}»? (да/нет): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
    if (confirm == "да")
    {
        db.DeletePlant(id);
        Console.WriteLine("Растение удалено.");
    }
    else
    {
        Console.WriteLine("Удаление отменено.");
    }
}

// ══════════════════════════════════════════════════════════
// Подменю отчётов
// ══════════════════════════════════════════════════════════

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine("  1 — Растения по теплицам");
        Console.WriteLine("  2 — Количество растений в теплицах");
        Console.WriteLine("  3 — Средняя высота по теплицам");
        Console.WriteLine("  0 — Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? "";

        switch (choice)
        {
            case "1": Report1_PlantsWithGreenhouses(db); break;
            case "2": Report2_CountByGreenhouse(db); break;
            case "3": Report3_AvgHeightByGreenhouse(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }

        Console.WriteLine();
    }
    while (choice != "0");
}

// ─────── Отчёт 1: Растения с названиями теплиц (JOIN) ───────
static void Report1_PlantsWithGreenhouses(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT p.plant_name, g.greenhouse_name, p.height_cm
                 FROM plant p
                 JOIN greenhouse g ON p.greenhouse_id = g.greenhouse_id
                 ORDER BY p.plant_name")
        .Title("Растения по теплицам")
        .Header("Название", "Теплица", "Высота (см)")
        .ColumnWidths(22, 20, 14)
        .Numbered()
        .Footer("Всего записей")
        .Print();
}

// ─────── Отчёт 2: Количество растений по теплицам (GROUP BY + COUNT) ───────
static void Report2_CountByGreenhouse(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT g.greenhouse_name, COUNT(*) AS cnt
                 FROM plant p
                 JOIN greenhouse g ON p.greenhouse_id = g.greenhouse_id
                 GROUP BY g.greenhouse_name
                 ORDER BY g.greenhouse_name")
        .Title("Количество растений по теплицам")
        .Header("Теплица", "Кол-во")
        .ColumnWidths(25, 10)
        .Print();
}

// ─────── Отчёт 3: Средняя высота по теплицам (GROUP BY + AVG) ───────
static void Report3_AvgHeightByGreenhouse(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT g.greenhouse_name,
                        ROUND(AVG(p.height_cm), 1) AS avg_height
                 FROM plant p
                 JOIN greenhouse g ON p.greenhouse_id = g.greenhouse_id
                 GROUP BY g.greenhouse_name
                 ORDER BY avg_height DESC")
        .Title("Средняя высота растений по теплицам")
        .Header("Теплица", "Средняя высота (см)")
        .ColumnWidths(25, 22)
        .Print();
}

// ══════════════════════════════════════════════════════════
// Фильтр по теплице
// ══════════════════════════════════════════════════════════

static void FilterByGreenhouse(DatabaseManager db)
{
    Console.WriteLine("--- Фильтр по теплице ---");
    Console.WriteLine("Доступные теплицы:");
    var greenhouses = db.GetAllGreenhouses();
    foreach (var g in greenhouses)
        Console.WriteLine("  " + g);

    Console.Write("Введите ID теплицы: ");
    if (!int.TryParse(Console.ReadLine(), out int ghId))
    {
        Console.WriteLine("Ошибка: введите целое число.");
        return;
    }

    var plants = db.GetPlantsByGreenhouse(ghId);
    if (plants.Count == 0)
    {
        Console.WriteLine("В этой теплице нет растений.");
        return;
    }

    Console.WriteLine($"\nРастения теплицы #{ghId}:");
    foreach (var p in plants)
        Console.WriteLine("  " + p);
    Console.WriteLine($"Итого: {plants.Count}");
}
