using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using AutoMapper;
using Domain.Entity;
using Domain.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class PessoaService
    {
        IPessoaRepository _pessoaRepository;
        IMapper _mapper;
        IAppLogger<PessoaService> _logger;
        private readonly IConfiguration _configuration;

        public PessoaService(IPessoaRepository pessoaRepository, IMapper mapper, IConfiguration configuration, IAppLogger<PessoaService> logger)
        {
            _pessoaRepository = pessoaRepository;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task AddPersonAsync(PessoaDTO pessoaDTO)
        {
            _logger.LogInformation($"Adicionando nova pessoa.");

            ValidatePerson(pessoaDTO);
            pessoaDTO.Password = HashPassword(pessoaDTO.Password);
            var pessoaToAdd = _mapper.Map<Pessoa>(pessoaDTO);
            await _pessoaRepository.AddAsync(pessoaToAdd);

            _logger.LogInformation($"Nova pessoa adicionada.");
        }

        public async Task ReactivatePersonByIdAsync(int id)
        {
            _logger.LogInformation($"Reativando pessoa com Id:{id}");
            Pessoa pessoa = await _pessoaRepository.GetByIdAsync(id);

            if (pessoa == null)
                throw new NotFoundException("Não existe pessoa com Id: " + id);

            pessoa.IsActive = true;
            _logger.LogInformation($"Pessoa com Id:{id} reativado");
            await _pessoaRepository.UpdateAsync(pessoa);
        }

        public async Task<LoggedDTO> LoginAsync(LoginDTO loginDTO)
        {
            _logger.LogInformation($"Efetuando login para {loginDTO.Email}.");
            loginDTO.Password = HashPassword(loginDTO.Password);
            Pessoa pessoa = await _pessoaRepository.GetByEmailAndPasswordAsync(loginDTO.Email, loginDTO.Password);
            if (pessoa == null)
                throw new NotFoundException("Email ou senha inválidos");
            if (!pessoa.IsActive)
                throw new BadDataException("Conta inativa.");

            LoggedDTO loggedDTO = new LoggedDTO() { Email = loginDTO.Email, Token = GenerateJwtToken(pessoa) };

            _logger.LogInformation($"Login efetuado para {loginDTO.Email}.");
            return loggedDTO;
        }

        private void ValidatePerson(PessoaDTO pessoa)
        {
            string errorMessage = "";
            errorMessage = ValidationHelper.ValidaEmpties<PessoaDTO>(pessoa, errorMessage);

            if (!IsEmailValid(pessoa.Email))
                errorMessage += "Email inválido. ";

            if (!IsPasswordValid(pessoa.Password))
                errorMessage += "Senha deve conter no mínimo de 8 caracteres com números, letras e caracteres especiais. ";

            if(!string.IsNullOrEmpty(errorMessage))
                throw new BadDataException(errorMessage.Trim());
        }

        

        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        private bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Pelo menos 8 caracteres, 1 letra, 1 número e 1 caractere especial
            var pattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&^+=_\-])[A-Za-z\d@$!%*#?&^+=_\-]{8,}$";
            return Regex.IsMatch(password, pattern);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            var guidBytes = new byte[16];
            Array.Copy(hash, guidBytes, 16);

            var guid = new Guid(guidBytes);
            return guid.ToString();
        }

        private string GenerateJwtToken(Pessoa pessoa)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, pessoa.Email),
                new Claim(ClaimTypes.Role, pessoa.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken
            (
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                issuer: _configuration["Jwt:Issuer"],
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
