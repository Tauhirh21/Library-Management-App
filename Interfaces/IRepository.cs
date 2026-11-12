namespace LibraryApp.Interfaces;

public interface IRepository<T>
{
    IEnumerable<T> GetAll();
    T? GetById(string id);
    void Add(T item);
    void Update(T item);
    void Remove(string id);
    void Save();
}
