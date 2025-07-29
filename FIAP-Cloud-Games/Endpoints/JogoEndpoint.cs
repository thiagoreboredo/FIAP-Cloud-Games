using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP_Cloud_Games.Endpoints
{
    public static class JogoEndpoint
    {
        public static void MapGameEndpoints(this WebApplication app)
        {
            var jogoMapGroup = app.MapGroup("/jogo").RequireAuthorization();
            jogoMapGroup.MapPost("/", CreateGame);
            jogoMapGroup.MapGet("/", GetAllGames).RequireAuthorization("Administrador");

            jogoMapGroup.MapDelete("/{id}", DeleteGame).RequireAuthorization("Administrador");
            jogoMapGroup.MapPut("/{id}", UpdateGame).RequireAuthorization("Administrador");
        }

        public static async Task<IResult> CreateGame([FromBody] JogoDTO jogoDTO, [FromServices] JogoService jogoService)
        {
            await jogoService.AddGameAsync(jogoDTO);
            return TypedResults.Created();
        }

        public static async Task<IResult> GetAllGames([FromServices] JogoService jogoService)
        {
            List<JogoDTO> jogos = await jogoService.GetAllGamesAsync();
            return TypedResults.Ok(jogos);
        }

        public static async Task<IResult> DeleteGame(int id, [FromServices] JogoService jogoService)
        {
            await jogoService.DeleteGameByIdAsync(id);
            return TypedResults.NoContent();
        }

        public static async Task<IResult> UpdateGame(int id, [FromBody] JogoDTO jogoDTO, [FromServices] JogoService jogoService)
        {
            await jogoService.UpdateGameByIdAsync(id, jogoDTO);
            return TypedResults.NoContent();
        }
    }
}