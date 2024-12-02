using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess
{
    public class BaseModel<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Contexto
        /// </summary>
        JujuTestContext _context;
        /// <summary>
        /// Entidad
        /// </summary>
        protected DbSet<TEntity> _dbSet;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public BaseModel(JujuTestContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }


        /// <summary>
        /// Consulta todas las entidades
        /// </summary>
        public virtual IQueryable<TEntity> GetAll
        {
            get { return _dbSet; }
        }

        /// <summary>
        /// Consulta una entidad por id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity FindById(object id)
        {
            return _dbSet.Find(id);
        }



        /// <summary>
        /// Crea un entidad (Guarda)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();

            return entity;
        }



        /// <summary>
        /// Actualiza la entidad (GUARDA)
        /// Correciones a tener encuenta:
        /// 1. Si el entidad original  o entidad esitada retorna crea una excepcion por ArgumentNull
        /// 2. Se elimino el parametro de salida out bool changed
        /// 3. Se valida primero si se realizaron cambios en el objeto antes de almacenarlos en la base de datos con la funcion SaveChangesAsync
        /// 4. Se convirtio en un metodo asincrono
        /// </summary>
        /// <param name="editedEntity">Entidad editada</param>
        /// <param name="originalEntity">Entidad Original sin cambios</param>
        /// <param name="changed">Indica si se modifico la entidad</param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity editedEntity, TEntity originalEntity)
        {
            if (editedEntity == null)
            {
                throw new ArgumentNullException("El objeto proporcionado para la edición no puede estar vacío o nulo.");
            }
            else if (originalEntity == null) 
            {
                throw new ArgumentNullException("No se encontró la entidad original en la base de datos. Verifique que el identificador proporcionado sea correcto.");
            }

            _context.Entry(originalEntity).CurrentValues.SetValues(editedEntity);

            var hasChanged = _context.Entry(originalEntity).State == EntityState.Modified;

            if (hasChanged)
            {
                await _context.SaveChangesAsync();
            }

            return originalEntity;
        }

       

        /// <summary>
        /// Elimina una entidad (Guarda)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Delete(TEntity entity)
        {
            _dbSet.Remove(entity);

            _context.SaveChanges();

            return entity;
        }


        /// <summary>
        /// Guardar cambios
        /// </summary>
        public virtual void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <summary>
        /// Verifica si existe un valor específico en una columna determinada.
        /// </summary>
        /// <typeparam name="TProperty">Tipo del valor de la columna.</typeparam>
        /// <param name="propertyExpression">Expresión lambda que define la columna (p. ej., x => x.Name).</param>
        /// <param name="value">Valor a buscar en la columna.</param>
        /// <returns>True si el valor existe, false en caso contrario.</returns>
        public async Task<bool> ExistsAsync<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression), "La expresión de propiedad no puede ser nula.");
            }

            // Extraer el nombre de la propiedad
            var propertyName = GetPropertyName(propertyExpression);

            // Crear una consulta LINQ dinámica usando EF.Property
            return await _dbSet.AnyAsync(entity =>
                EF.Property<TProperty>(entity, propertyName).Equals(value));
        }

        /// <summary>
        /// Verifica si existe un valor específico en una columna determinada.
        /// </summary>
        /// <typeparam name="TProperty">Tipo del valor de la columna.</typeparam>
        /// <param name="propertyExpression">Expresión lambda que define la columna (p. ej., x => x.Name).</param>
        /// <param name="value">Valor a buscar en la columna.</param>
        /// <returns>Retorna la entidad filtrada.</returns>
        public async Task<IQueryable<TEntity>> GetEntityAsync<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression), "La expresión de propiedad no puede ser nula.");
            }

            var propertyName = GetPropertyName(propertyExpression);

            // Crear una consulta LINQ dinámica usando EF.Property
            return _dbSet.Where(entity => EF.Property<TProperty>(entity, propertyName).Equals(value));
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad desde una expresión lambda.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="propertyExpression">Expresión lambda (p. ej., x => x.Name).</param>
        /// <returns>El nombre de la propiedad.</returns>
        private static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("La expresión proporcionada no es válida. Debe ser una expresión de miembro, como x => x.Propiedad.", nameof(propertyExpression));
        }

        /// <summary>
        /// Crea múltiples entidades en la base de datos en una única operación.
        /// </summary>
        /// <param name="entities">Lista de entidades a ser creadas.</param>
        /// <exception cref="ArgumentNullException">Se lanza si la lista de entidades es nula o está vacía.</exception>
        public async Task CreateMultipleAsync(List<TEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities), "La lista de entidades no puede estar vacía.");
            }

            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}
