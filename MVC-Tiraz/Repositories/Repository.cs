namespace TIRAZ.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            IQueryable<T> query = _dbSet;
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllIncludingAsync(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>[]? includes = null, params string[] includePaths)
        {
            IQueryable<T> query = _dbSet;

            foreach (var path in includePaths) query = query.Include(path);
            if (predicate != null) query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id) =>
            await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
            predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);

        public async Task AddAsync(T entity) =>
            await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) =>
            await _dbSet.AddRangeAsync(entities);

        public void Update(T entity) =>
            _dbSet.Update(entity);

        public void Remove(T entity) =>
            _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) =>
            _dbSet.RemoveRange(entities);

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
