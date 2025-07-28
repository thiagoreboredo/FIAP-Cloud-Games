using Domain.Entity.Enum;
using Domain.Repository;

namespace Domain.Entity
{
    public class Jogo : EntityBase, IAggregateRoot
    {
        public string Nome { get; set; }
        public string Empresa { get; set; }
        public double Preco { get; set; }

        public EClassificacao Classificacao { get; set; }
        public EGenero Genero { get; set; }

        public ICollection<Promocao> Promocoes { get; set; }

        public ICollection<Pessoa> Pessoas { get; set; }
    }
}
