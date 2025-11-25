using System.Text.Json.Serialization;

namespace JewelryShop.DTO
{
    public class GoogleLoginRequest
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }
}
