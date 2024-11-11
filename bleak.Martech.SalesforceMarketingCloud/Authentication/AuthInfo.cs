using System.ComponentModel.DataAnnotations;

namespace bleak.Martech.SalesforceMarketingCloud
{
    public class AuthInfo : IAuthInfo
    {
        [Required]
        public string Endpoint { get; set; } = string.Empty;
        
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [Range(0, int.MaxValue)]
        public int? ClientId { get; set; }


        const string DevEndpoint = "https://localhost/service.asmx";
        public bool DevMode
        {
            get
            {
                return Endpoint.ToLower() == DevEndpoint.ToLower();
            }
        }
    }
}