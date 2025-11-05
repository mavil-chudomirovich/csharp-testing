using Application.AppExceptions;
using Application.Repositories;
using Domain.Commons;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        protected readonly IGreenWheelDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(IGreenWheelDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<Guid> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entityFromDb = await GetByIdAsync(id)
        ?? throw new NotFoundException($"{typeof(T).Name} is not found");

            if (entityFromDb is SorfDeletedEntity softEntity && softEntity.DeletedAt == null)
            {
                softEntity.DeletedAt = DateTime.UtcNow;
            }
            else
            {
                _dbSet.Remove(entityFromDb);
            }
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, object>>[]? includes = null)
        {
            var query = _dbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            if (typeof(SorfDeletedEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Cast<SorfDeletedEntity>()
                             .Where(x => x.DeletedAt == null)
                             .Cast<T>();
            }

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            //return  await _dbSet.FirstOrDefault(t => t.Id == id && t.);
            var entityFromDb = await _dbSet.FindAsync(id);
            if (entityFromDb is SorfDeletedEntity softEntity1 && softEntity1.DeletedAt == null)
            {
                return entityFromDb;
            }
            else if (entityFromDb is SorfDeletedEntity softEntity2 && softEntity2.DeletedAt != null)
            {
                return null;
            }
            return entityFromDb;
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            var entityFromDb = await GetByIdAsync(entity.Id);
            if (entityFromDb == null)
            {
                throw new NotFoundException($"{typeof(T).Name} is not found");
            }
            _dbContext.Entry(entityFromDb).CurrentValues.SetValues(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            await _dbSet.AddRangeAsync(entities);
        }
        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }
    }
}