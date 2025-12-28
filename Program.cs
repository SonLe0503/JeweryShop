using JewelryShop.Filters;
using JewelryShop.Hubs;
using JewelryShop.Mappers;
using JewelryShop.Models;
using JewelryShop.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();

    builder.EntitySet<User>("Users");
    builder.EntitySet<Product>("Products");
    builder.EntitySet<Category>("Categories");
    builder.EntitySet<Collection>("Collections");

    return builder.GetEdmModel();
}
builder.Services.AddScoped<EmailVerificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("https://shop.hijean.io.vn", "https://hijean.io.vn", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
builder.Services.AddDbContext<JewelryShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JewelryShop API",
        Version = "v1",
        Description = "API Authentication with JWT for JewelryShop"
    });

    // Thêm cấu hình bảo mật cho JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, 
        Scheme = "Bearer",               
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token ở dạng: Bearer {token}"
    });

    // Áp dụng yêu cầu bảo mật cho tất cả endpoint có [Authorize]
    c.OperationFilter<AuthorizeCheckOperationFilter>();
});
builder.Services.AddControllers()
    .AddOData(opt =>
    {
        opt.Select()
           .Filter()
           .OrderBy()
           .Expand()
           .Count()
           .SetMaxTop(100)
           .AddRouteComponents("odata", GetEdmModel());
    });
builder.Services.AddAutoMapper(typeof(RegisterProfile).Assembly);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    //var validAudiences = jwtSettings.GetSection("Audience").Get<string[]>();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});
builder.Services.AddSignalR();
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chathub").RequireCors("AllowAll");

app.Run();
