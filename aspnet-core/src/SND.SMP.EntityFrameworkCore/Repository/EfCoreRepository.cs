using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class EFCoreRepository<T> : IEfCoreRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public EFCoreRepository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public T GetById(long id)
    {
        return _dbSet.Find(id);
    }

    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }
}

public interface IEfCoreRepository<T> where T : class
{
    T GetById(long id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}