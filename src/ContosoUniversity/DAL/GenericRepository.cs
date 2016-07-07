using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ContosoUniversity.DAL
{
    public class GenericRepository<TEntity> where TEntity : class
    {

        internal DbContext context;
        internal DbSet<TEntity> DbSet;

        public GenericRepository(DbContext context)
        {
            this.context = context;
            DbSet = context.Set<TEntity>();
        }

        internal async Task<List<TEntity>> Get(IQueryable<TEntity> query = null,
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null, 
        int? page = null,
        int? pageSize = null
        )
        {
            if (query == null)
            {
                query = DbSet;
            }

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            if (page != null && pageSize != null)
                query = query
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);

            return await query.ToListAsync();
        }

        public async Task<TEntity> GetById(IQueryable<TEntity> query = null, params object[] id)
        {
            if (query == null)
            {
                query = DbSet;
            }

            // Look in the database
            return await FindAsync(query, id);

        }

        public async Task<TEntity> Insert(TEntity entity)
        {
            context.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<int> Delete(params object[] id)
        {
            IQueryable<TEntity> query = DbSet;

            // Look in the database
            var entityToDelete = await FindAsync(query, id);

            DbSet.Remove(entityToDelete);
            return await context.SaveChangesAsync();
        }

        public async Task<int> Delete(TEntity entityToDelete)
        {
            DbSet.Remove(entityToDelete);
            return await context.SaveChangesAsync();
        }

        public async Task<TEntity> Update(TEntity entity)
        {
            // context.Update will set the current Entries state to modified
            context.Update(entity);
            await context.SaveChangesAsync();
            return entity;

        }

        private async Task<TEntity> FindAsync(IQueryable<TEntity> query, params object[] id) 
        {

            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var key = entityType.FindPrimaryKey();
            string PKName = key.Properties.FirstOrDefault().Name;
            var entries = context.ChangeTracker.Entries<TEntity>();

            var i = 0;
            foreach (var property in key.Properties)
            {
                entries = entries.Where(e => e.Property(property.Name).CurrentValue == id[i]);
                i++;
            }

            var entry = entries.FirstOrDefault();
            if (entry != null)
            {
                // Return the local object if it exists.
                return await Task.Run(() => entry.Entity);
            }

            // TODO: Build the real LINQ Expression
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            query = query.Where((Expression<Func<TEntity, bool>>)
                Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, PKName),
                        Expression.Constant(id[0])),
                    parameter));

            // Look in the database
            return await Task.Run(() => query.FirstOrDefault());
        }
    }
}
