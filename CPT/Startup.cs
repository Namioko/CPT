using System.Web.Http;
using Owin;
using System.Net.Http.Formatting;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using CPT;

[assembly: OwinStartup(typeof(Startup))]

namespace CPT
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var httpConfiguration = new HttpConfiguration();

            SetFormatters(httpConfiguration.Formatters);

            httpConfiguration.MapHttpAttributeRoutes();
            appBuilder.UseWebApi(httpConfiguration);
        }

        private void SetFormatters(MediaTypeFormatterCollection formatters)
        {
            formatters.Clear();

            formatters.Add(new JsonMediaTypeFormatter
            {
                SerializerSettings = Json()
            });

            //formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/x-www-form-urlencoded"));
        }

        public static JsonSerializerSettings Json(TypeNameHandling typeNameHandling = TypeNameHandling.None)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = typeNameHandling,
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>()
            };

            settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return settings;
        }
    }
}