using Microsoft.Extensions.DependencyInjection;
using SoowGood.Domain.Service.Repositories.OtpService;
using SoowGood.Domain.Service.Repositories.User;
using SoowGood.Domain.Service.Repositories.UserRole;
using SoowGood.Insfracture.Service.ImplementRepositories.OtpService;
using SoowGood.Insfracture.Service.ImplementRepositories.User;
using SoowGood.Insfracture.Service.ImplementRepositories.UserRole;
using SoowGoodWeb.Application.Service.Services.Password;
using SoowGoodWeb.Application.Service.Services.Patient;
using SoowGoodWeb.Application.Service.Services.SMSService;
using SoowGoodWeb.Application.Service.Services.TokenService;
using SoowGoodWeb.Application.Service.Services.UserRole;
using SoowGoodWeb.Domain.Service.Repositories;
using SoowGoodWeb.Domain.Service.Repositories.Appointment;
using SoowGoodWeb.Domain.Service.Repositories.PatientRepository;
using SoowGoodWeb.Domain.Service.Repositories.RestApiCallService;
using SoowGoodWeb.Domain.Service.Repositories.SignUp;
using SoowGoodWeb.Domain.Service.Repositories.UserRole;
using SoowGoodWeb.Insfracture.Service.DataAccessService;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories.Appointment;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories.PatientRepository;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories.RestApiCallService;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories.SignUp;
using SoowGoodWeb.Insfracture.Service.ImplementRepositories.UserRole;

namespace SoowGood.DI.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiServices(this IServiceCollection services)
        {
            #region Repository DI
            services.AddScoped<SqlDataAccessLayer>();
            services.AddScoped<IAuthenticationQueryRepository, AuthenticationQueryRepository>();
            services.AddScoped<IUserRoleQueryRepository, UserRoleQueryRepository>();
            services.AddScoped<IUserCommanRepository, UserCommanRepository>();
            services.AddScoped<IOtpUserQueryService, OtpUserQueryService>();
            services.AddScoped<IPatientQueryRepository, PatientQueryRepository>();
            services.AddScoped<IUserRoleManagementCommandRepository, UserRoleManagementCommandRepository>();
            services.AddScoped<IAppointmentQueryRepository, AppointmentQueryRepository>();
            services.AddScoped<IBaseRestClientApiService, BaseRestClientApiService>();
            services.AddScoped<ISignUpRepository, SignUpRepository>();




            #endregion Repository DI



            #region Services DI

            services.AddScoped<UserRoleService>();
            services.AddScoped<TokenService>();
            services.AddScoped<GreenWebSmsService>();
            services.AddScoped<PatientService>();
            services.AddScoped<PasswordHasherService>();

            #endregion Services DI

            return services;
        }
    }
}
