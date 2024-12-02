using DataAccess;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Business
{
    public class BaseService<TEntity> where TEntity : class, new()
    {
        protected BaseModel<TEntity> _BaseModel;

        public BaseService(BaseModel<TEntity> baseModel)
        {
            _BaseModel = baseModel;
        }

        #region Repository


        /// <summary>
        /// Consulta todas las entidades
        /// </summary>
        public virtual async Task<IQueryable<TEntity>> GetAll()
        {
            return _BaseModel.GetAll;
        }

        /// <summary>
        /// Crea un entidad (Guarda)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Create(TEntity entity)
        {

            return await _BaseModel.Create(entity);
        }


        /// <summary>
        /// Actualiza la entidad (GUARDA)
        /// 1. Se elimino el parametro de salida out bool changed
        /// 2. Se convirtio en un metodo asincrono
        /// </summary>
        /// <param name="editedEntity">Entidad editada</param>
        /// <param name="originalEntity">Entidad Original sin cambios</param>
        /// <param name="changed">Indica si se modifico la entidad</param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(object id, TEntity editedEntity)
        {
            TEntity originalEntity = _BaseModel.FindById(id);
            return await _BaseModel.Update(editedEntity, originalEntity);
        }


        /// <summary>
        /// Elimina una entidad (Guarda)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Delete(TEntity entity)
        {
            return await _BaseModel.Delete(entity);
        }

        /// <summary>
        /// Guardar cambios
        /// </summary>
        public virtual void SaveChanges()
        {
            _BaseModel.SaveChanges();
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad desde una expresión lambda.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="propertyExpression">Expresión lambda (p. ej., x => x.Name).</param>
        /// <returns>El nombre de la propiedad.</returns>
        public async Task<bool> ExistsAsync<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            return await _BaseModel.ExistsAsync(propertyExpression, value);
        }

        /// <summary>
        /// Obtiene el nombre de la propiedad desde una expresión lambda.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <typeparam name="TProperty">Tipo de la propiedad.</typeparam>
        /// <param name="propertyExpression">Expresión lambda (p. ej., x => x.Name).</param>
        /// <returns>El nombre de la propiedad.</returns>
        public async Task<IQueryable<TEntity>> GetEntityAsync<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
        {
            return await _BaseModel.GetEntityAsync(propertyExpression, value);
        }

        /// <summary>
        /// Crea múltiples entidades en la base de datos en una única operación.
        /// </summary>
        /// <param name="entities">Lista de entidades a ser creadas.</param>
        public async Task CreateMultipleAsync(List<TEntity> entities)
        {
            await _BaseModel.CreateMultipleAsync(entities);
        }
        #endregion


    }
}
