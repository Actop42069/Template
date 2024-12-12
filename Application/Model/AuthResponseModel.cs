using Domain.Enumeration;

namespace Application.Model
{
    public class AuthResponseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AuthTokenType TokenType { get; set; }
        public string Token { get; set; }
    }
}
