using System.Text.Json;
using LibraryApp.Interfaces;

namespace LibraryApp.Persistence;

public class JsonRepository<T> : IRepository<T>
{
    private readonly string _path;
    private readonly Func<T, string> _idSelector;
    private readonly List<T> _items;
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public JsonRepository(string path, Func<T, string> idSelector)
    {
        _path = path;
        _idSelector = idSelector;

        if (File.Exists(_path))
        {
            var json = File.ReadAllText(_path);
            _items = JsonSerializer.Deserialize<List<T>>(json, _opts) ?? new();
        }
        else
        {
            _items = new();
        }
    }

    public IEnumerable<T> GetAll() => _items.ToList();

    public T? GetById(string id) =>
        _items.FirstOrDefault(x => _idSelector(x) == id);

    public void Add(T item) { _items.Add(item); Save(); }

    public void Update(T item)
    {
        var idx = _items.FindIndex(x => _idSelector(x) == _idSelector(item));
        if (idx >= 0) _items[idx] = item;
        Save();
    }

    public void Remove(string id)
    {
        _items.RemoveAll(x => _idSelector(x) == id);
        Save();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(_path, JsonSerializer.Serialize(_items, _opts));
    }
}
