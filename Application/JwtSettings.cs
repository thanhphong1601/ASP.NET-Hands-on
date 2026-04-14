using System;
using System.Collections.Generic;
using System.Text;

namespace Application
{
    public class JwtSettings
    {
        string Issuer { get; set; }
        string Audience { get; set; }
        string SecretKey { get; set; }
    }
}
