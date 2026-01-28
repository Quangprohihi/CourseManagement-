using CourseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly CourseManagementContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(CourseManagementContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.ToList();
        public T? GetById(object id) => _dbSet.Find(id);
        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null) _dbSet.Remove(entity);
        }
    }
}