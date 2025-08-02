using Domain.Entity.Enum;
using Domain.Repository;

namespace Domain.Entity
{
    public class Pessoa : EntityBase, IAggregateRoot
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public ERole Role { get; set; }
        public ICollection<Jogo> Games { get; set; }
    }
}
