using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP_Cloud_Games.Endpoints
{
    public static class PessoaEndpoint
    {
        public static void MapPersonEndpoints(this WebApplication app)
        {
            var pessoaMapGroup = app.MapGroup("/pessoa");

            pessoaMapGroup.MapPost("/", CreatePerson);
            pessoaMapGroup.MapPost("/login", Login);
            pessoaMapGroup.MapPatch("/reativar/{id}", ReactivatePerson).RequireAuthorization("Administrador");
        }

        public static async Task<IResult> CreatePerson([FromBody] PessoaDTO pessoaDTO, [FromServices] PessoaService pessoaService)
        {
            await pessoaService.AddPersonAsync(pessoaDTO);
            return TypedResults.Created();
        }

        public static async Task<IResult> Login([FromBody] LoginDTO loginDTO, [FromServices] PessoaService pessoaService)
        {
            LoggedDTO loggedDTO = await pessoaService.LoginAsync(loginDTO);
            return TypedResults.Ok(loggedDTO);
        }

        public static async Task<IResult> ReactivatePerson(int id, [FromServices] PessoaService pessoaService)
        {
            await pessoaService.ReactivatePersonByIdAsync(id);
            return TypedResults.NoContent();
        }
    }
}