/// <summary>
/// Растение (основная таблица, сторона «много»)
/// </summary>
class Plant
{
    /// <summary>Идентификатор растения</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор теплицы (внешний ключ)</summary>
    public int GreenhouseId { get; set; }

    /// <summary>Название растения</summary>
    public string Name { get; set; }

    private double _heightCm;

    /// <summary>
    /// Высота растения в сантиметрах (не может быть отрицательной)
    /// </summary>
    public double HeightCm
    {
        get => _heightCm;
        set
        {
            if (value < 0)
                throw new ArgumentException(
                    "Высота растения не может быть отрицательной");
            _heightCm = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Plant(int id, int greenhouseId, string name, double heightCm)
    {
        Id = id;
        GreenhouseId = greenhouseId;
        Name = name;
        HeightCm = heightCm;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Plant() : this(0, 0, "", 0) { }

    public override string ToString()
        => $"[{Id}] {Name}, теплица #{GreenhouseId}, высота: {HeightCm} см";
}
