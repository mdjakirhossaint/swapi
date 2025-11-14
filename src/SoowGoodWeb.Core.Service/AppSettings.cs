using Microsoft.Extensions.Configuration;

namespace SoowGoodWeb.Core.Service
{
    public static class AppSettings
    {
        private static IConfigurationRoot configuration;

        static AppSettings()
        {
            configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
               .Build();
        }

        public static string ConnectionStringForDapper => configuration.GetSection("ConnectionStrings:Default").Value;
        public static string ConnectionStringForEntityFramework => configuration.GetSection("ConnectionStrings:EntityConnection").Value;
        public static string Token => configuration.GetSection("Settings:Token").Value;
        public static string PublicApiKey => configuration.GetSection("Settings:PublicApiKey").Value;
        public static string TokenExpireTimeInSecound => configuration.GetSection("Settings:TokenExpireTimeInSecound").Value;
    }
}
