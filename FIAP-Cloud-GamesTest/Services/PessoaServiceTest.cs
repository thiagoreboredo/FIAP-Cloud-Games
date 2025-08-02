using Application.DTOs;
using Application.Exceptions;
using Application.Helper; // Adicionado
using Application.Services;
using AutoMapper;
using Domain.Entity;
using Domain.Repository;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FIAP_Cloud_GamesTest.Services
{
    public class PessoaServiceTest
    {
        private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IConfiguration> _configurationMock = new();
        private readonly Mock<IAppLogger<PessoaService>> _appLoggerMock = new();

        private PessoaService CreateService()
        {
            return new PessoaService(
                _pessoaRepositoryMock.Object,
                _mapperMock.Object,
                _configurationMock.Object,
                _appLoggerMock.Object
            );
        }

        [Fact] 
        public async Task AddPessoa_DeveAdicionarPessoa_QuandoDadosValidos()
        {
            // Arrange
            var service = CreateService();
            var pessoaDTO = new PessoaDTO
            {
                Email = "test@test.com",
                Password = "Senha!123",
                Name = "Teste"
            };

            _mapperMock
                .Setup(m => m.Map<Pessoa>(It.IsAny<PessoaDTO>()))
                .Returns(new Pessoa());

            // Act
            await service.AddPersonAsync(pessoaDTO);

            // Assert
            _pessoaRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Pessoa>()), Times.Once);
        }

        [Fact]
        public async Task AddPessoa_DeveReceberException_QuandoDadosNulos()
        {

            // Arrange
            var service = CreateService();
            var pessoaDTO = new PessoaDTO
            {
                Email = "",
                Password = "",
                Name = ""
            };

            _mapperMock
                .Setup(m => m.Map<Pessoa>(It.IsAny<PessoaDTO>()))
                .Returns(new Pessoa());

            // Act
            var exception = await Assert.ThrowsAsync<BadDataException>(() => service.AddPersonAsync(pessoaDTO));

            // Assert
            Assert.Contains("Nome não pode ser nulo. Email não pode ser nulo. Senha não pode ser nulo.", exception.Message);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("1234567")]
        [InlineData("semCaracterEspecial1")]
        public async Task AddPessoa_DeveReceberException_QuandoSenhaInvalida(string senhaInvalida)
        {

            // Arrange
            var service = CreateService();
            var pessoaDTO = new PessoaDTO
            {
                Email = "test@test.com",
                Password = senhaInvalida,
                Name = "Test"
            };

            _mapperMock
                .Setup(m => m.Map<Pessoa>(It.IsAny<PessoaDTO>()))
                .Returns(new Pessoa());

            // Act
            var exception = await Assert.ThrowsAsync<BadDataException>(() => service.AddPersonAsync(pessoaDTO));

            // Assert
            Assert.Contains("Senha deve conter no mínimo de 8 caracteres com números, letras e caracteres especiais.", exception.Message);
        }

        [Fact]
        public async Task ReactivatePessoaById_DeveReativarPessoa_QuandoDadosIdExiste()
        {
            // Arrange
            var service = CreateService();
            int id = 1;
            Pessoa pessoa = new Pessoa() { Id = id, IsActive = false };
            _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(pessoa);


            // Act
            await service.ReactivatePersonByIdAsync(id);

            // Assert
            Assert.True(pessoa.IsActive);
            _pessoaRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Pessoa>()), Times.Once);
        }

        [Fact]
        public async Task ReactivatePessoaById_DeveReceberException_QuandoDadosIdNaoExiste()
        {
            // Arrange
            var service = CreateService();
            int id = 1;
            _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Pessoa) null);


            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.ReactivatePersonByIdAsync(id));

            // Assert
            Assert.Contains($"Não existe pessoa com Id: {id}", exception.Message);
        }

        [Fact]
        public async Task Login_DeveSerEfetuadoComSucesso_PessoaAtiva()
        {
            // Arrange
            var service = CreateService();
            LoginDTO loginDTO = new LoginDTO() { Email = "test@test.com", Password = "Senha@123" };
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("12345678901234567890123456789012");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("IssuerTest");
            _configurationMock.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("60");

            _pessoaRepositoryMock
                .Setup(r => r.GetByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Pessoa() { Email = loginDTO.Email, Password = loginDTO.Password, IsActive = true });

            // Act
            var result = await service.LoginAsync(loginDTO);

            // Assert
            Assert.Equal(loginDTO.Email, result.Email);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task Login_DeveReceberException_EmailOuSenhaNaoEncontrados()
        {
            // Arrange
            var service = CreateService();
            LoginDTO loginDTO = new LoginDTO() { Email = "test@test.com", Password = "Senha@123" };

            _pessoaRepositoryMock
                .Setup(r => r.GetByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Pessoa) null);

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.LoginAsync(loginDTO));

            // Assert
            Assert.Contains($"Email ou senha inválidos", exception.Message);
        }

        [Fact]
        public async Task Login_DeveReceberException_PessoaInvalida()
        {
            // Arrange
            var service = CreateService();
            LoginDTO loginDTO = new LoginDTO() { Email = "test@test.com", Password = "Senha@123" };

            _pessoaRepositoryMock
                .Setup(r => r.GetByEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Pessoa() { Email = loginDTO.Email, Password = loginDTO.Password, IsActive = false });

            // Act
            var exception = await Assert.ThrowsAsync<BadDataException>(() => service.LoginAsync(loginDTO));

            // Assert
            Assert.Contains($"Conta inativa.", exception.Message);
        }
    }
}
