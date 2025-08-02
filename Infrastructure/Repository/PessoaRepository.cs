using Domain.Entity;
using Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class PessoaRepository : EFRepository<Pessoa>, IPessoaRepository
    {
        public PessoaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Pessoa> GetByEmailAndPasswordAsync(string email, string password)
        {

            return await _dbSet.FirstOrDefaultAsync(entity => entity.Email == email && entity.Password == password);
        }
    }
}
