using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ECSMandate.Startup))]
namespace ECSMandate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
