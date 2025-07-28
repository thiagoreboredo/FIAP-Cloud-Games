using Domain.Entity;

namespace Domain.Repository
{
    public interface IPessoaRepository : IRepository<Pessoa>
    {
        Task<Pessoa> GetByEmailAndPasswordAsync(string email, string senha);
    }
}
