using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;

namespace ConfigurationSettings
{
    /// <summary>
    /// POCO for MySqlSettings from appsettings.json
    /// </summary>
    public class MySqlSettings
    {
        public string? InternalContainerHostname { get; set; }
        public string? Port { get; set;  }
        public string? UserId{ get; set; }
        public string? Password{ get; set; }
        public string? Database{ get; set; }

    }
}
