using Microsoft.AspNetCore.Mvc;
using NoSeProgramarJajajLoL.models;
using NoSeProgramarJajajLoL.Services;

namespace NoSeProgramarJajajLoL.Controllers
{
    [ApiController]
    [Route("api/poke")] // Esto define la URL base: localhost:xxxx/api/poke
    public class PokeController : ControllerBase
    {
        private readonly PokeServices _pokeServices;

        public PokeController(PokeServices pokeServices)
        {
            _pokeServices = pokeServices;
        }

        // GET: api/poke?limit=20
        [HttpGet]
        public async Task<ActionResult<List<Pokemon>>> GetAll([FromQuery] int limit = 20)
        {
            var result = await _pokeServices.GetPokemonsAsync(limit);
            return Ok(result);
        }

        // GET: api/poke/pikachu  O  api/poke/25
        [HttpGet("{term}")]
        public async Task<ActionResult<Pokemon>> GetByTerm(string term)
        {
            var pokemon = await _pokeServices.GetPokemonByTermAsync(term);

            if (pokemon == null)
            {
                return NotFound(new { message = $"No se encontró el pokemon: {term}" });
            }

            return Ok(pokemon);
        }
    }
}