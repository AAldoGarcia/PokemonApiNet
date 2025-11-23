using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoSeProgramarJajajLoL.models;
using NoSeProgramarJajajLoL.Services;

namespace NoSeProgramarJajajLoL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokeController : ControllerBase
    {
        private readonly PokeServices _pokeServices;
        public PokeController(PokeServices pokeServices)
        {
            _pokeServices = pokeServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 20)
        {
            try
            {
                var pokemons = await _pokeServices.GetPokemonsAsync(limit);
                return Ok(pokemons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var pokemon = await _pokeServices.GetPokemonByIdAsync(id);
                if (pokemon == null)
                    return NotFound($"Pokemon with id {id} not found");
                return Ok(pokemon);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}