using SoowGoodWeb.Core.Service.GenericModels;

namespace SoowGood.Domain.Service.Repositories.BaseInterface
{
    public interface IBaseCommandRepository<T> where T : class
    {
        Task<Response<bool>> Insert(T entity);
        Task<Response<bool>> Update(T entity);
        Task<Response<bool>> Delete(T entity);
    }
}
