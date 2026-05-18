/// <summary>
/// Теплица (справочная таблица, сторона «один»)
/// </summary>
class Greenhouse
{
    /// <summary>Идентификатор теплицы</summary>
    public int Id { get; set; }

    /// <summary>Название теплицы</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Greenhouse(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Greenhouse() : this(0, "") { }

    public override string ToString() => $"[{Id}] {Name}";
}
