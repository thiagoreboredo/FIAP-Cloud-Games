using Domain.Entity.Enum;

namespace Application.DTOs
{
    public class JogoDTO
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public double Price { get; set; }

        public EClassificacao Rating { get; set; }
        public EGenero Genre { get; set; }
    }
}
