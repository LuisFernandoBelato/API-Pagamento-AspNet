using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.MariaDB.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

#region VARIÁVEIS DE AMBIENTE

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

string pathAppsettings = "appsettings.json";

if (env == "Development")
{
    pathAppsettings = "appsettings.Development.json";
}

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(pathAppsettings)
    .Build();

Environment.SetEnvironmentVariable("STRING_CONEXAO", config.GetSection("stringConexao").Value);
#endregion


var builder = WebApplication.CreateBuilder(args);

# region Configuração do Serilog

var logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
Directory.CreateDirectory(logFolder);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Error()  
    .WriteTo.File(new CompactJsonFormatter(),
           Path.Combine(logFolder, "log-.json"),  
           retainedFileCountLimit: 10,  
           rollingInterval: RollingInterval.Day) 
    .WriteTo.File(
           Path.Combine(logFolder, "log-.log"),  
           retainedFileCountLimit: 10,
           rollingInterval: RollingInterval.Day)
    .WriteTo.MariaDB(builder.Configuration.GetSection("stringConexao").Value,
                   tableName: "Logs",
                   autoCreateTable: true)  
   .CreateLogger();
#endregion

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API - GATEWAY PARA PAGAMENTOS",
        Version = "v1",
        Description = $@"<h3>ROTAS PARA VALIDAÇÃO E CADASTRO DE <b>CARTÕES E TRANSAÇÕES</b></h3>
                        <p>
                            Validação de Cartão - Cálculo para Obtenção de Parcelas - Geração de Transações - Manutenção do Status das Transações
                        </p>",
        Contact = new OpenApiContact
        {
            Name = "Suporte Unoeste",
            Email = string.Empty,
            Url = new Uri("https://www.unoeste.br"),
        },
    });
    // Definindo o esquema de segurança
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira o token JWT no formato *Bearer {token}*",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // Adiciona a segurança ao Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


//Habilitar o uso do serilog.
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();

#region AUTENTICAÇÃO VIA JWT 

// *** Adiciona o Middleware de autenticação e autorização
//Estamos falando para o ASP.NET
//que agora também queremos verificar o cabeçalho da requisição
//para buscar um Token ou algo do tipo.
builder.Services
    .AddAuthentication(x =>
    {
        //Especificando o Padrão do Token

        //para definir que o esquema de autenticação que queremos utilizar é o Bearer e o
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        //Diz ao asp.net que utilizamos uma autenticação interna,
        //ou seja, ela é gerada neste servidor e vale para este servidor apenas.
        //Não é gerado pelo google/fb
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(x =>
    {
        //Lendo o Token

        // Obriga uso do HTTPs
        x.RequireHttpsMetadata = false;

        // Configurações para leitura do Token
        x.TokenValidationParameters = new TokenValidationParameters
        {
            // Chave que usamos para gerar o Token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("minha-chave-secreta-minha-chave-secreta")),
            ValidAudience = "Usuários da API",
            ValidIssuer = "Unoeste",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

//política
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("APIAuth", new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser().Build());
});

#endregion


#region IOC 

//adicionado ao IOC por requisição

builder.Services.AddScoped(typeof(Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Services.CartaoService));
builder.Services.AddScoped(typeof(Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Services.PagamentoService));

//adicionar ao IOC instância únicas (singleton)
builder.Services.AddSingleton<Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.BD>(new Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.BD());


#endregion


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// *** Usa o Middleware de autenticação e autorização
app.UseAuthorization();
app.UseAuthentication();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.RoutePrefix = ""; //habilitar a página inicial da API ser a doc.
    c.DocumentTitle = "Gerenciamento de GATEWAY DE PAGAMENTOS - API V1";
});



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


#region CRIANDO O USUÁRIO PARA SER AUTENTICADO

//Contém as Afirmação (claims) do token.
//As Claimns são declarações sobre uma entidade (geralmente o usuário) e metadados adicionais.
//São objetos do tipo “Chave/Valor” que definem uma afirmação,
//como por exemplo, dizer que o nome do usuário é "André".
var userClaims = new List<Claim>();
userClaims.Add(new Claim(ClaimTypes.Name, "Luis Fernando Belato")); //Claim padrão
userClaims.Add(new Claim(ClaimTypes.Role, "Administrador")); //Claim padrão
userClaims.Add(new Claim("id", "123456"));
userClaims.Add(new Claim("cpf", "11111111111")); //sensível (?)  / pode-se criptografar
userClaims.Add(new Claim("data", DateTime.Now.ToString()));
userClaims.Add(new Claim("email", "luisfer@gmail.com"));
userClaims.Add(new Claim("contratoId", "11111111"));

//Juntar as Claims em um conjunto.
var identidade = new ClaimsIdentity(userClaims);

//Iniciando a criãção do token
var handler = new JwtSecurityTokenHandler();

//Criação da Chave de Assinatura (Signing Key):
string minhaKey = "minha-chave-secreta-minha-chave-secreta"; // >= 32 caracteres
SecurityKey key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.ASCII.GetBytes(minhaKey));
SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


//criando um descriptor: Objeto que reune o cabeçalho, payload e assinatura do token.
var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
{
    Audience = "Usuários da API", //Quem vai usar o token, público-alvo
    Issuer = "Unoeste", //Emisssor
    NotBefore = DateTime.Now, //Data de início
    Expires = DateTime.Now.AddYears(1), //Data fim 
    Subject = identidade, // credenciais de assinatura + Claims
    SigningCredentials = signingCredentials //a chave para criptografar os dados
};

//Criação do Token JWT
var dadosToken = handler.CreateToken(tokenDescriptor);

//gerando o token (encripta e gera ao token)
string jwt = handler.WriteToken(dadosToken);

Console.Write(jwt);

#endregion


app.Run();