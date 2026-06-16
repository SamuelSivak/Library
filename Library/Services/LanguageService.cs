using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Library.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentLanguage()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "SK";

            if (context.Request.Headers.TryGetValue("Accept-Language", out var value))
            {
                var langHeader = value.ToString();
                if (!string.IsNullOrEmpty(langHeader))
                {
                    var lang = langHeader.Split(',')
                                         .Select(l => l.Split(';')[0].Trim().ToUpper())
                                         .FirstOrDefault();
                    if (!string.IsNullOrEmpty(lang))
                    {
                        if (lang.Contains('-'))
                        {
                            lang = lang.Split('-')[0];
                        }

                        if (lang == "SK") return "SK";
                        if (lang == "GR" || lang == "EL") return "GR";
                        if (lang == "EN") return "EN";
                    }
                }
            }

            return "SK"; 
        }
    }
}
