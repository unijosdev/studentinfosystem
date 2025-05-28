using AutoMapper;
using Microsoft.Owin;
using Owin;
using SwiftKampus.ViewModels;
using SwiftKampusModel;

[assembly: OwinStartupAttribute(typeof(SwiftKampus.Startup))]
namespace SwiftKampus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<StudentIndexVM, Student>();
                //cfg.CreateMap<Bar, BarDto>();
            });
        }
    }
}
