using BusinessLogic.Services;
using Infrastructure.Models;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Implements;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeleMedicine_BE.Utils;
using System.Reflection;
using System.IO;
using Microsoft.OpenApi.Any;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using BeautyAtHome.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TeleMedicine_BE
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "firebase_admin_sdk.json");
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(pathToKey)
            });

            services.AddDbContext<TeleMedicineContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DBConnection")));

            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped(typeof(IPagingSupport<>), typeof(PagingSupport<>));
            services.AddScoped(typeof(IPagingSupport<>), typeof(PagingSupport<>));
            services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();

            services.AddTransient<ISymptomRepository, SymptomRepository>();
            services.AddTransient<ISymptomService, SymptomService>();

            services.AddTransient<IHospitalRepository, HospitalRepository>();
            services.AddTransient<IHospitalService, HospitalService>();

            services.AddTransient<IDrugTypeRepository, DrugTypeRepository>();
            services.AddTransient<IDrugTypeService, DrugTypeService>();

            services.AddTransient<IDrugRepository, DrugRepository>();
            services.AddTransient<IDrugService, DrugService>();

            services.AddTransient<IMajorRepository, MajorRepository>();
            services.AddTransient<IMajorService, MajorService>();

            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IRoleService, RoleService>();

            services.AddTransient<IDiseaseGroupRepository, DiseaseGroupRepository>();
            services.AddTransient<IDiseaseGroupService, DiseaseGroupService>();

            services.AddTransient<IDiseaseRepository, DiseaseRepository>();
            services.AddTransient<IDiseaseService, DiseaseService>();

            services.AddTransient<ITimeFrameRepository, TimeFrameRepository>();
            services.AddTransient<ITimeFrameService, TimeFrameService>();

            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<INotificationService, NotificationService>();

            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IAccountService, AccountService>();

            services.AddTransient<ICertificationRepository, CertificationRepository>();
            services.AddTransient<ICertificationService, CertificationService>();

            services.AddTransient<IDoctorRepository, DoctorRepository>();
            services.AddTransient<IDoctorService, DoctorService>();

            services.AddTransient<IPatientRepository, PatientRepository>();
            services.AddTransient<IPatientService, PatientService>();

            services.AddTransient<ISlotRepository, SlotRepository>();
            services.AddTransient<ISlotService, SlotService>();

            services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["jwt:Key"])),
                    ValidAudience = Configuration["jwt:Audience"],
                    ValidIssuer = Configuration["jwt:Issuer"],
                };
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            }).AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Telemedicine", Version = "v1" });
                c.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("00:00:00")
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                      },
                      new List<string>()
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeleMedicine_BE v1"));

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }   
    }
}
