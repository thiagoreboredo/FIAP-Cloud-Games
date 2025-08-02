namespace Domain.Entity
{
    public class Promocao : EntityBase
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DiscountPercentage { get; set; }

        public ICollection<Jogo> Games { get; set; }
    }
}
