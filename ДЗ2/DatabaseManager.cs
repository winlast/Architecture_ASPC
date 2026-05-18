using Microsoft.Data.Sqlite;

/// <summary>
/// Управление базой данных SQLite.
/// Инкапсулирует все операции с БД: создание таблиц,
/// импорт CSV, CRUD-операции, выполнение запросов для отчётов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    /// <summary>
    /// Конструктор. Принимает путь к файлу БД.
    /// </summary>
    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    // ──────────── Инициализация ────────────

    /// <summary>
    /// Создаёт таблицы (если не существуют) и загружает CSV при первом запуске
    /// </summary>
    public void InitializeDatabase(string greenhouseCsvPath, string plantCsvPath)
    {
        CreateTables();

        if (GetAllGreenhouses().Count == 0 && File.Exists(greenhouseCsvPath))
        {
            ImportGreenhousesFromCsv(greenhouseCsvPath);
            Console.WriteLine($"[OK] Загружены теплицы из {greenhouseCsvPath}");
        }

        if (GetAllPlants().Count == 0 && File.Exists(plantCsvPath))
        {
            ImportPlantsFromCsv(plantCsvPath);
            Console.WriteLine($"[OK] Загружены растения из {plantCsvPath}");
        }
    }

    /// <summary>Создание таблиц</summary>
    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS greenhouse (
                greenhouse_id INTEGER PRIMARY KEY AUTOINCREMENT,
                greenhouse_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS plant (
                plant_id INTEGER PRIMARY KEY AUTOINCREMENT,
                greenhouse_id INTEGER NOT NULL,
                plant_name TEXT NOT NULL,
                height_cm REAL NOT NULL,
                FOREIGN KEY (greenhouse_id) REFERENCES greenhouse(greenhouse_id)
            );";
        cmd.ExecuteNonQuery();
    }

    /// <summary>Импорт теплиц из CSV</summary>
    private void ImportGreenhousesFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO greenhouse (greenhouse_id, greenhouse_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1].Trim());
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>Импорт растений из CSV</summary>
    private void ImportPlantsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO plant (plant_id, greenhouse_id, plant_name, height_cm)
                VALUES (@id, @ghId, @name, @height)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@ghId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2].Trim());
            cmd.Parameters.AddWithValue("@height", double.Parse(parts[3].Trim(),
                System.Globalization.CultureInfo.InvariantCulture));
            cmd.ExecuteNonQuery();
        }
    }

    // ──────────── Чтение данных ────────────

    /// <summary>Получить все теплицы</summary>
    public List<Greenhouse> GetAllGreenhouses()
    {
        var result = new List<Greenhouse>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT greenhouse_id, greenhouse_name FROM greenhouse ORDER BY greenhouse_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new Greenhouse(reader.GetInt32(0), reader.GetString(1)));
        return result;
    }

    /// <summary>Получить все растения</summary>
    public List<Plant> GetAllPlants()
    {
        var result = new List<Plant>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT plant_id, greenhouse_id, plant_name, height_cm FROM plant ORDER BY plant_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new Plant(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)));
        return result;
    }

    /// <summary>Получить растение по Id</summary>
    public Plant GetPlantById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT plant_id, greenhouse_id, plant_name, height_cm FROM plant WHERE plant_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return new Plant(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetDouble(3));
        return null;
    }

    // ──────────── Изменение данных ────────────

    /// <summary>Добавить растение (Id генерируется автоматически)</summary>
    public void AddPlant(Plant plant)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO plant (greenhouse_id, plant_name, height_cm)
            VALUES (@ghId, @name, @height)";
        cmd.Parameters.AddWithValue("@ghId", plant.GreenhouseId);
        cmd.Parameters.AddWithValue("@name", plant.Name);
        cmd.Parameters.AddWithValue("@height", plant.HeightCm);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Обновить данные растения</summary>
    public void UpdatePlant(Plant plant)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE plant
            SET greenhouse_id = @ghId, plant_name = @name, height_cm = @height
            WHERE plant_id = @id";
        cmd.Parameters.AddWithValue("@id", plant.Id);
        cmd.Parameters.AddWithValue("@ghId", plant.GreenhouseId);
        cmd.Parameters.AddWithValue("@name", plant.Name);
        cmd.Parameters.AddWithValue("@height", plant.HeightCm);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Удалить растение по Id</summary>
    public void DeletePlant(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM plant WHERE plant_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ──────────── Выполнение произвольного запроса (для отчётов) ────────────

    /// <summary>
    /// Выполняет SQL-запрос и возвращает имена столбцов и строки результата.
    /// Используется классом ReportBuilder.
    /// </summary>
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }

        return (columns, rows);
    }

    // ──────────── [ГРУППА Г] Фильтр по теплице ────────────

    /// <summary>Получить растения конкретной теплицы</summary>
    public List<Plant> GetPlantsByGreenhouse(int greenhouseId)
    {
        var result = new List<Plant>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT plant_id, greenhouse_id, plant_name, height_cm
            FROM plant WHERE greenhouse_id = @ghId ORDER BY plant_name";
        cmd.Parameters.AddWithValue("@ghId", greenhouseId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new Plant(
                reader.GetInt32(0), reader.GetInt32(1),
                reader.GetString(2), reader.GetDouble(3)));
        return result;
    }
}
