using Application.DTOs;
using Application.Exceptions;
using Application.Helper;
using Application.Services;
using AutoMapper;
using Domain.Entity;
using Domain.Entity.Enum;
using Domain.Repository;
using Moq;
using TechTalk.SpecFlow;

[Binding]
public class JogoServiceSteps
{
    private readonly ScenarioContext _context;
    private readonly Mock<IJogoRepository> _jogoRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAppLogger<JogoService>> _appLoggerMock = new();
    private JogoService _service;
    private JogoDTO _jogoDto;
    private Exception _exception;

    public JogoServiceSteps(ScenarioContext context)
    {
        _context = context;
        _service = new JogoService(
            _jogoRepositoryMock.Object,
            _mapperMock.Object,
            _appLoggerMock.Object);
    }

    [Given(@"um jogo com nome ""(.*)"", empresa ""(.*)"", preco (.*), classificacao (.*) e genero (.*)")]
    public void GivenUmJogoComNome(string nome, string empresa, double preco, int classificacao, int genero)
    {
        _jogoDto = new JogoDTO
        {
            Name = nome,
            Company = empresa,
            Price = preco,
            Rating = (EClassificacao) classificacao,
            Genre = (EGenero) genero
        };
    }

    [Given(@"um jogo existente com id (\d+)")]
    public void GivenUmJogoExistenteComId(int id)
    {
        var jogo = new Jogo
        {
            Id = id,
            Name = "Existente",
            Company = "DevCompany",
            Price = 100,
            Rating = EClassificacao.Livre,
            Genre = EGenero.Action
        };

        _jogoRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(jogo);
    }

    [Given(@"um jogo DTO com nome ""(.*)"", empresa ""(.*)"", preco (.*), classificacao (.*) e genero (.*)")]
    public void GivenUmJogoDTOParaAtualizar(string nome, string empresa, double preco, int classificacao, int genero)
    {
        _jogoDto = new JogoDTO
        {
            Name = nome,
            Company = empresa,
            Price = preco,
            Rating = (EClassificacao) classificacao,
            Genre = (EGenero) genero
        };
    }

    [When(@"eu adicionar o jogo")]
    public async Task WhenEuAdicionarOJogo()
    {
        _mapperMock.Setup(m => m.Map<Jogo>(_jogoDto)).Returns(new Jogo());
        await _service.AddGameAsync(_jogoDto);
    }

    [When(@"eu atualizar o jogo com id (\d+)")]
    public async Task WhenEuAtualizarOJogoComId(int id)
    {
        await _service.UpdateGameByIdAsync(id, _jogoDto);
    }

    [When(@"eu tentar deletar o jogo com id (\d+)")]
    public async Task WhenEuTentarDeletarOJogoComId(int id)
    {
        try
        {
            await _service.DeleteGameByIdAsync(id);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [Then(@"o repositorio deve ter recebido uma chamada para adicionar o jogo")]
    public void ThenRepositorioDeveReceberChamadaAdd()
    {
        _jogoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Jogo>()), Times.Once);
    }

    [Then(@"o repositorio deve ter recebido uma chamada para atualizar o jogo")]
    public void ThenRepositorioDeveReceberChamadaUpdate()
    {
        _jogoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Jogo>()), Times.Once);
    }

    [Then(@"uma excecao NotFoundException deve ser lancada")]
    public void ThenExcecaoNotFoundDeveSerLancada()
    {
        Assert.IsType<NotFoundException>(_exception);
    }
}
