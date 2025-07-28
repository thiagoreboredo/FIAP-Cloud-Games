using Domain.Entity.Enum;

namespace Application.DTOs
{
    public class PessoaDTO
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public ERole Role { get; set; }

    }
}
