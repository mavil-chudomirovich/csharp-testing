using API.BackgroundJob;
using API.Extentions;
using API.Filters;
using API.Middleware;
using API.Middlewares;
using Application;
using Application.Abstractions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Mappers;
using Application.Repositories;
using Application.UnitOfWorks;
using Application.Validators.User;
using AutoMapper;
using CloudinaryDotNet;
using DotNetEnv;
using FluentValidation;
using Infrastructure.ExternalService;
using Infrastructure.Interceptor;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (builder.Environment.IsDevelopment())
            {
                Env.Load("../.env");
                builder.Configuration.AddJsonFile("appsettings.json", optional: true);
                builder.Configuration.AddJsonFile($"appsettings.Development.json", optional: true);
            }
            builder.Configuration.AddEnvironmentVariables();

            // Frontend Url
            var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
                ?? "http://localhost:3000";
            var frontendPublicOrigin = Environment.GetEnvironmentVariable("FRONTEND_PUBLIC_ORIGIN")
                ?? frontendOrigin;

            // Add services to the container.
            // Add services to the container.
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "GreenWheel API",
                    Version = "v1",
                    Description = "Tài liệu Swagger cho hệ thống thuê xe GreenWheel"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header sử dụng scheme Bearer.
                        Nhập vào chuỗi như sau: 'Bearer <token>'.",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(frontendOrigin, frontendPublicOrigin)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            // Kết nối DB
            var connectionString = builder.Configuration["MSSQL_CONNECTION_STRING"];
            builder.Services.AddInfrastructue(connectionString!);

            // Cache
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["REDIS_CONFIGURATION"];
                options.InstanceName = builder.Configuration["REDIS_INSTANCE_NAME"]
                                       ?? "GreenWheel:"; // fallback
            });

            //thêm httpcontextAccessor để lấy context trong service
            builder.Services.AddHttpContextAccessor();
            //Add repositories
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IOTPRepository, OTPRepository>();
            builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            builder.Services.AddScoped<IJwtBlackListRepository, JwtBlackListRepository>();
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IVehicleModelRepository, VehicleModelRepository>();
            builder.Services.AddScoped<ICitizenIdentityRepository, CitizenIdentityRepository>();
            builder.Services.AddScoped<IDriverLicenseRepository, DriverLicenseRepository>();
            builder.Services.AddScoped<IRentalContractRepository, RentalContractRepository>();
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
            builder.Services.AddScoped<IDepositRepository, DepositRepository>();
            builder.Services.AddScoped<IStationRepository, StationRepository>();
            builder.Services.AddScoped<IMomoPaymentLinkRepository, MomoPaymentRepository>();
            builder.Services.AddScoped<IModelImageRepository, ModelImageRepository>();
            builder.Services.AddScoped<IVehicleSegmentRepository, VehicleSegmentRepository>();
            builder.Services.AddScoped<ICloudinaryRepository, CloudinaryRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();
            builder.Services.AddScoped<IVehicleCheckListRepository, VehicleChecklistRepository>();
            builder.Services.AddScoped<IVehicleChecklistItemRepository, VehicleChecklistItemRepository>();
            builder.Services.AddScoped<IStationFeedbackRepository, StationFeedbackRepository>();
            builder.Services.AddScoped<IVehicleComponentRepository, VehicleComponentRepository>();
            builder.Services.AddScoped<IBusinessVariableRepository, BusinessVariableRepository>();
            builder.Services.AddScoped<IModelComponentRepository, ModelComponentRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            //Add Services
            builder.Services.AddScoped<IVehicleChecklistService, VehicleChecklistService>();
            builder.Services.AddScoped<IVehicleSegmentService, VehicleSegmentService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGoogleCredentialService, GoogleCredentialService>();
            builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IRentalContractService, RentalContractService>();
            builder.Services.AddScoped<IStationService, StationService>();
            builder.Services.AddScoped<ICitizenIdentityService, CitizenIdentityService>();
            builder.Services.AddScoped<IDriverLicenseService, DriverLicenseService>();
            builder.Services.AddScoped<IModelImageService, ModelImageService>();
            builder.Services.AddScoped<IPhotoService, CloudinaryService>();
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IStationFeedbackService, StationFeedbackService>();
            builder.Services.AddScoped<IChecklistItemImageService, ChecklistItemImageService>();
            builder.Services.AddScoped<IDispatchRequestService, DispatchService>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<IDispatchRepository, DispatchRepository>();
            builder.Services.AddScoped<IEmailSerivce, EmailService>();
            builder.Services.AddScoped<IAuthService, AuthSerivce>();
            builder.Services.AddScoped<IUserProfileSerivce, UserProfileSerivce>();
            builder.Services.AddScoped<IStatisticService, StatisticService>();
            builder.Services.AddScoped<IVehicleComponentService, VehicleComponentService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<IBusinessVariableService, BusinessVariableService>();
            //Interceptor
            builder.Services.AddScoped<UpdateTimestampInterceptor>();
            //Add Client
            builder.Services.AddHttpClient<IMomoService, MomoService>();
            builder.Services.AddHttpClient<IGeminiService, GeminiService>();
            //UOW
            builder.Services.AddScoped<IUnitOfwork, UnitOfwork>();
            builder.Services.AddScoped<IRentalContractUow, RentalContractUow>();
            builder.Services.AddScoped<IInvoiceUow, InvoiceUow>();
            builder.Services.AddScoped<IMediaUow, MediaUow>();
            builder.Services.AddScoped<IModelImageUow, ModelImageUow>();
            builder.Services.AddScoped<IVehicleChecklistUow, VehicleChecklistUow>();
            builder.Services.AddScoped<IVehicleModelUow, VehicleModelUow>();
            //Mapper
            builder.Services.AddAutoMapper(typeof(UserProfile)); // auto mapper sẽ tự động scan hết assembly đó và xem tất cả thằng kết thừa Profile rồi tạo lun
                                                                 // mình chỉ cần truyền một thằng đại diện thoi
                                                                 //configure <-> setting
                                                                 //Momo
            builder.Services.Configure<MomoSettings>(builder.Configuration.GetSection("MomoSettings"));
            //JWT
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            var _jwtSetting = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            builder.Services.AddJwtTokenValidation(_jwtSetting!);
            //Ratelimit
            builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection("RateLimitSettings"));
            //Email
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            //Otp
            builder.Services.Configure<OTPSettings>(builder.Configuration.GetSection("OTPSettings"));
            //Google
            var trmp = builder.Configuration.GetSection("GoogleAuthSettings");
            builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuthSettings"));
            //Gemini
            builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
            //Cloudinary
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            //middleware
            builder.Services.AddScoped<GlobalErrorHandlerMiddleware>();
            //sử dụng cache
            builder.Services.AddMemoryCache();
            //background job
            builder.Services.AddQuartz(q =>
            {
                // JOB 1: LateReturnWarningJob
                q.AddJob<LateReturnWarningJob>(opts =>
                    opts.WithIdentity("LateReturnWarningJob"));

                // Trigger chạy ngay
                q.AddTrigger(opts => opts
                    .ForJob("LateReturnWarningJob")
                    .WithIdentity("LateReturnWarningJob-Immediate")
                    .StartNow());

                // Trigger chạy 00:00 mỗi ngày
                q.AddTrigger(opts => opts
                    .ForJob("LateReturnWarningJob")
                    .WithIdentity("LateReturnWarningJob-Daily")
                    .WithCronSchedule("0 0 0 * * ?"));

                // JOB 2: ExpiredContractCleanupJob
                q.AddJob<ExpiredRentalContracCleanupJob>(opts =>
                    opts.WithIdentity("ExpiredRentalContracCleanupJob"));

                // Trigger chạy ngay
                q.AddTrigger(opts => opts
                    .ForJob("ExpiredRentalContracCleanupJob")
                    .WithIdentity("ExpiredRentalContracCleanupJob-Immediate")
                    .StartNow());

                // Trigger chạy 00:00 mỗi ngày
                q.AddTrigger(opts => opts
                    .ForJob("ExpiredRentalContracCleanupJob")
                    .WithIdentity("ExpiredRentalContracCleanupJob-Daily")
                    .WithCronSchedule("0 0 0 * * ?"));
            });

            // chạy background quartz
            builder.Services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });

            //thêm filter cho validation
            builder.Services.AddControllers(options =>
            {
                // Thêm ValidationFilter vào pipeline
                options.Filters.Add<ValidationFilter>();
            });
            //Fluentvalidator
            builder.Services.AddValidatorsFromAssemblyContaining(typeof(UserLoginReqValidator));
            //tắt validator tự ném lỗi
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            //khai báo sử dụng DI cho cloudinary

            //Cấu hình request nhận request, nó tự chuyển trường của các đối tượng trong
            //DTO thành snakeCase để binding giá trị, và lúc trả ra
            //thì các trường trong respone cũng sẽ bị chỉnh thành snake case
            //Ảnh hưởng khi map từ json sang object và object về json : json <-> object
            // builder.Services.AddControllers()
            // .AddJsonOptions(options =>
            // {
            //     options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
            //     options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            // });

            // đk cloudinary
            var account = new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret
            );
            var cloudinary = new Cloudinary(account)
            {
                Api = { Secure = true }
            };
            builder.Services.AddSingleton(cloudinary);

            var app = builder.Build();

            //run cache and add list roll to cache
            using (var scope = app.Services.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                var roleRepo = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();
                var roles = await roleRepo.GetAllAsync();
                cache.Set(Common.SystemCache.AllRoles, roles, new MemoryCacheEntryOptions
                {
                    //cache này sẽ tồn tại suốt vòng đời của cache
                    Priority = CacheItemPriority.NeverRemove
                });
                var businessVariableRepo = scope.ServiceProvider.GetRequiredService<IBusinessVariableRepository>();
                var businessVariables = await businessVariableRepo.GetAllAsync();
                //set cache và đảm bảo nó chạy xuyên suốt app
                cache.Set(Common.SystemCache.BusinessVariables, businessVariables, new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<GlobalErrorHandlerMiddleware>();
            // app.UseMiddleware<RateLimitMiddleware>();
            //if (builder.Environment.IsDevelopment())
            //    app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}