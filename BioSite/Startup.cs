using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BioSite.Startup))]
namespace BioSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
