using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory; // <-- IMPORTANTE
using NoSeProgramarJajajLoL.models;

namespace NoSeProgramarJajajLoL.Services
{
    public class PokeServices
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache; // <-- Inyectamos la caché
        private const string BaseUrl = "https://pokeapi.co/api/v2/";

        // Constructor actualizado
        public PokeServices(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        // 1. OBTENER LISTA (AHORA CON CACHÉ)
        public async Task<List<Pokemon>> GetPokemonsAsync(int limit = 20)
        {
            // Creamos una clave única. Ejemplo: "pokemons_list_1300"
            string cacheKey = $"pokemons_list_{limit}";

            // PREGUNTAMOS: ¿Existe esta lista en caché?
            if (!_cache.TryGetValue(cacheKey, out List<Pokemon> cachedPokemons))
            {
                // SI NO EXISTE: Ejecutamos tu lógica original (lenta)
                try
                {
                    // Aumentamos el Timeout del HttpClient por si acaso (para la primera carga)
                    _httpClient.Timeout = TimeSpan.FromMinutes(5);

                    var response = await _httpClient.GetFromJsonAsync<PokemonListResponse>($"{BaseUrl}pokemon?limit={limit}");

                    if (response?.Results == null) return new List<Pokemon>();

                    // NOTA: Hacer 1300 peticiones de golpe puede ser detectado como ataque.
                    // Lo dejamos así por ahora, pero ten en cuenta que la primera vez tardará.
                    var tasks = response.Results.Select(item => GetPokemonByTermAsync(item.Name));
                    var results = await Task.WhenAll(tasks);

                    cachedPokemons = results.Where(p => p != null).ToList()!;

                    // GUARDAMOS EN CACHÉ
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Mantiene los datos en memoria por 1 hora
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                        // Si nadie los usa en 20 minutos, los borra para ahorrar RAM
                        .SetSlidingExpiration(TimeSpan.FromMinutes(20));

                    _cache.Set(cacheKey, cachedPokemons, cacheEntryOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error obteniendo datos: {ex.Message}");
                    return new List<Pokemon>();
                }
            }

            // Devolvemos los datos (ya sea de la caché o recién descargados)
            return cachedPokemons;
        }

        // 2. BUSCAR POR NOMBRE O ID (También podemos cachear búsquedas individuales)
        public async Task<Pokemon?> GetPokemonByTermAsync(string term)
        {
            var cleanTerm = term.Trim().ToLower();
            string cacheKey = $"pokemon_detail_{cleanTerm}";

            if (!_cache.TryGetValue(cacheKey, out Pokemon? cachedPokemon))
            {
                try
                {
                    var data = await _httpClient.GetFromJsonAsync<PokemonApiResponse>($"{BaseUrl}pokemon/{cleanTerm}");

                    if (data == null) return null;

                    cachedPokemon = new Pokemon
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

                    // Guardamos el pokemon individual en caché por 1 hora
                    _cache.Set(cacheKey, cachedPokemon, TimeSpan.FromHours(1));
                }
                catch
                {
                    return null;
                }
            }

            return cachedPokemon;
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