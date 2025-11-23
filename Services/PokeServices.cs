using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NoSeProgramarJajajLoL.models;

namespace NoSeProgramarJajajLoL.Services
{
    public class PokeServices
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://pokeapi.co/api/v2/";

        public PokeServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Pokemon>> GetPokemonsAsync(int limit = 20)
        {
            var response = await _httpClient.GetFromJsonAsync<PokemonListResponse>($"{BaseUrl}pokemon?limit={limit}");

            if (response?.Results == null)
                return new List<Pokemon>();

            var pokemons = new List<Pokemon>();

            // Procesar en paralelo para mejor rendimiento
            var tasks = response.Results.Select(async item =>
            {
                var pokemonDetail = await GetPokemonDetailAsync(item.Url);
                return pokemonDetail;
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(p => p != null).ToList()!;
        }

        private async Task<Pokemon?> GetPokemonDetailAsync(string url)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<PokemonApiResponse>(url);
                if (data == null) return null;

                return new Pokemon
                {
                    Id = data.Id,
                    Name = data.Name,
                    SpriteUrl = data.Sprites?.FrontDefault ?? "",
                    Types = data.Types?.Select(t => t.Type.Name).ToList() ?? new List<string>(),
                    Stats = data.Stats?.ToDictionary(
                        s => s.Stat.Name,
                        s => s.BaseStat
                    ) ?? new Dictionary<string, int>()
                };
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error getting pokemon detail: {ex.Message}");
                return null;
            }
        }

        public async Task<Pokemon?> GetPokemonByIdAsync(int id)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<PokemonApiResponse>($"{BaseUrl}pokemon/{id}");
                if (data == null) return null;

                return new Pokemon
                {
                    Id = data.Id,
                    Name = data.Name,
                    SpriteUrl = data.Sprites?.FrontDefault ?? "",
                    Types = data.Types?.Select(t => t.Type.Name).ToList() ?? new List<string>(),
                    Stats = data.Stats?.ToDictionary(
                        s => s.Stat.Name,
                        s => s.BaseStat
                    ) ?? new Dictionary<string, int>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pokemon by id: {ex.Message}");
                return null;
            }
        }
    }

    // CLASES CORREGIDAS CON JSONPropertyName
    public class PokemonListResponse
    {
        [JsonPropertyName("results")]
        public List<PokemonListItem> Results { get; set; } = new();
    }

    public class PokemonListItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class PokemonApiResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("types")]
        public List<PokemonTypeSlot> Types { get; set; } = new();

        [JsonPropertyName("sprites")]
        public PokemonSprites? Sprites { get; set; }

        [JsonPropertyName("stats")]
        public List<PokemonStat> Stats { get; set; } = new();
    }

    public class PokemonTypeSlot
    {
        [JsonPropertyName("type")]
        public TypeInfo Type { get; set; } = new();
    }

    public class TypeInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class PokemonSprites
    {
        [JsonPropertyName("front_default")]
        public string FrontDefault { get; set; } = string.Empty;
    }

    public class PokemonStat
    {
        [JsonPropertyName("base_stat")]
        public int BaseStat { get; set; }

        [JsonPropertyName("stat")]
        public StatInfo Stat { get; set; } = new();
    }

    public class StatInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}