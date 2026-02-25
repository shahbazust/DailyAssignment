namespace StudentManagement.Repository;

public interface IRepository<T, TKey>
{
    T? GetById(TKey id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(TKey id);
}