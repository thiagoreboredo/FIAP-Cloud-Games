using Application.DTOs;
using Application.Services;

namespace FIAP_Cloud_Games.Endpoints
{
    public static class PessoaEndpoint
    {
        public static void MapPersonEndpoints(this WebApplication app)
        {
            var pessoaMapGroup = app.MapGroup("/pessoa");

            pessoaMapGroup.MapPost("/", CreatePerson);
            pessoaMapGroup.MapPost("/login", Login);
            pessoaMapGroup.MapPatch("/reativar/id", ReactivatePerson).RequireAuthorization("Administrador");
        }

        public static async Task<IResult> CreatePerson(PessoaDTO pessoaDTO, PessoaService pessoaService)
        {
            await pessoaService.AddPersonAsync(pessoaDTO);            
            return TypedResults.Created();
        }

        public static async Task<IResult> Login(LoginDTO loginDTO, PessoaService pessoaService)
        {
            LoggedDTO loggedDTO = await pessoaService.LoginAsync(loginDTO);
            return TypedResults.Ok(loggedDTO);
        }

        public static async Task<IResult> ReactivatePerson(int id, PessoaService pessoaService)
        {
            await pessoaService.ReactivatePersonByIdAsync(id);
            return TypedResults.NoContent();
        }


    }
}
