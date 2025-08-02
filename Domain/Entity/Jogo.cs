using Domain.Entity.Enum;
using Domain.Repository;

namespace Domain.Entity
{
    public class Jogo : EntityBase, IAggregateRoot
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public double Price { get; set; }

        public EClassificacao Rating { get; set; }
        public EGenero Genre { get; set; }

        public ICollection<Promocao> Promotions { get; set; }

        public ICollection<Pessoa> People { get; set; }
    }
}
