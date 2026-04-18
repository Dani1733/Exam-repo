using System.Text.Json.Serialization;

namespace examBE_MovieCatalog_DH.DTOs
{
    public class EditMovieDTO
    {
        [JsonPropertyName("movieId")]
        public string MovieId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
