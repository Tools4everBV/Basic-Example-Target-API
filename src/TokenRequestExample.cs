using EXAMPLE.API.Controllers;
using Swashbuckle.AspNetCore.Filters;

namespace EXAMPLE.API
{
    public class TokenRequestExample : IExamplesProvider<object>
    {
        private readonly IConfiguration _config;

        public TokenRequestExample(IConfiguration config)
        {
            _config = config;
        }

        public object GetExamples()
        {
            return new TokenRequest
            {
                ClientId = _config["ApplicationSettings:ClientId"],
                ClientSecret = _config["ApplicationSettings:ClientSecret"]
            };
        }
    }
}
