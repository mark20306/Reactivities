using API.Extensions;
using API.Middleware;
using API.SignalR;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opt => 
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseXContentTypeOptions(); //設置 X-Content-Type-Options 標頭，防止瀏覽器對服務器返回的 MIME 類型進行猜測。
app.UseReferrerPolicy(opt => opt.NoReferrer()); //設置 Referrer-Policy 標頭，控制瀏覽器在發送 HTTP 請求時包含的 Referer 標頭，設置為不傳遞 Referer。
app.UseXXssProtection(opt => opt.EnabledWithBlockMode()); //啟用內建的瀏覽器 XSS 防護機制，並設置為阻止頁面載入時檢測到 XSS 攻擊，啟用了阻止模式。
app.UseXfo(opt => opt.Deny()); //設置 X-Frame-Options 標頭，防止網站被放入 iframe 中，設置為拒絕任何 iframe。
app.UseCsp(opt => opt //設置 Content-Security-Policy (CSP) 標頭。
    .BlockAllMixedContent() //阻止所有混合內容載入。
    .StyleSources(s => s.Self().CustomSources("https://fonts.googleapis.com")) //指定允許載入樣式表的來源，允許從同一域名（.Self()）載入樣式表，並且使用了 .CustomSources 方法添加了額外的自定義來源。
    .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "data:")) //指定允許載入字型的來源。
    .FormActions(s => s.Self()) //指定允許表單動作發送到同一域名。
    .FrameAncestors(s => s.Self()) //指定允許 iframe 的祖先是同一域名。
    .ImageSources(s => s.Self().CustomSources("blob:", "https://res.cloudinary.com")) //指定允許載入圖像的來源。
    .ScriptSources(s => s.Self()) //指定允許從同一域名載入腳本。
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else{
    app.Use(async (context, next) => //ASP.NET Core 中使用中介軟體的方式。這段程式碼將一個匿名的中介軟體添加到 ASP.NET Core 的請求處理管道中。
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000"); //使用了 Headers.Add() 方法來向 HTTP 響應中添加 Strict-Transport-Security 標頭。Strict-Transport-Security 標頭告訴瀏覽器僅使用 HTTPS 加密通訊，並指定了最大有效期為一年（即 31536000 秒）。
        await next.Invoke(); //使用了 next.Invoke() 方法來執行下一個中介軟體，即請求處理管道中的後續中介軟體。
    });
    //在每個 HTTP 回應中添加 Strict-Transport-Security 標頭，從而告訴瀏覽器在未來一年內強制使用 HTTPS 連接。這有助於提高網站的安全性，防止中間人攻擊和其他類型的攻擊。
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>(); 
    var userManager = services.GetRequiredService<UserManager<AppUser>>(); 
    await context.Database.MigrateAsync();
    await Seed.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex , "An error occured during migration");
    
}

app.Run();
