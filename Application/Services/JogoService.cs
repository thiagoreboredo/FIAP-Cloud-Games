using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using AutoMapper;
using Domain.Entity;
using Domain.Repository;
namespace Application.Services
{
    public class JogoService
    {
        IJogoRepository _jogoRepository;
        IMapper _mapper;
        IAppLogger<JogoService> _logger;
        public JogoService(IJogoRepository jogoRepository, IMapper mapper, IAppLogger<JogoService> logger)
        {
            _jogoRepository = jogoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task DeleteGameByIdAsync(int id)
        {
            _logger.LogInformation($"Deletando jogo com id: {id}.");
            Jogo jogo = await _jogoRepository.GetByIdAsync(id);

            if (jogo == null)
                throw new NotFoundException("Não existe jogo com Id: " + id);

            await _jogoRepository.DeleteAsync(jogo);
            _logger.LogInformation($"Jogo com id {id} deletado.");
        }

        public async Task UpdateGameByIdAsync(int id, JogoDTO jogoDTO)
        {
            _logger.LogInformation($"Atualizando jogo com id: {id}.");

            ValidateJogo(jogoDTO);

            Jogo jogo = await _jogoRepository.GetByIdAsync(id);

            if (jogo == null)
                throw new NotFoundException("Não existe jogo com Id: " + id);

            jogo.Nome = jogoDTO.Name;
            jogo.Empresa = jogoDTO.Company;
            jogo.Preco = jogoDTO.Price;
            jogo.Classificacao = jogoDTO.Rating;
            jogo.Genero = jogoDTO.Genre;

            await _jogoRepository.UpdateAsync(jogo);

            _logger.LogInformation($"Jogo com id {id} atualizado.");
        }

        public async Task AddGameAsync(JogoDTO jogoDTO)
        {
            _logger.LogInformation("Criando jogo.");

            ValidateJogo(jogoDTO);
            Jogo jogo = _mapper.Map<Jogo>(jogoDTO);
            await _jogoRepository.AddAsync(jogo);

            _logger.LogInformation("Jogo criado.");
        }

        private void ValidateJogo(JogoDTO jogo)
        {
            string errorMessage = "";
            errorMessage = ValidationHelper.ValidaEmpties<JogoDTO>(jogo, errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new BadDataException(errorMessage.Trim());
        }

        public async Task<List<JogoDTO>> GetAllGamesAsync()
        {
            _logger.LogInformation("Buscando todos os jogos.");
            List<Jogo> jogos = (await _jogoRepository.GetAllAsync()).ToList();
            _logger.LogInformation($"{jogos.Count} jogos retonaram.");
            return _mapper.Map<List<JogoDTO>>(jogos);
        }
    }
}
