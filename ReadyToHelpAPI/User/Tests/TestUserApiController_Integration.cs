namespace readytohelpapi.User.Tests;

using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using readytohelpapi.Common.Data;
using readytohelpapi.Common.Tests;
using readytohelpapi.User.Models;
using Xunit;

public partial class Program { }

[Trait("Category", "Integration")]
public class TestUserApiController_Integration
    : IClassFixture<WebApplicationFactory<Program>>,
        IClassFixture<DbFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DbFixture _dbFixture;

    public TestUserApiController_Integration(
        WebApplicationFactory<Program> factory,
        DbFixture dbFixture
    )
    {
        _factory = factory;
        _dbFixture = dbFixture;

        _dbFixture.ResetDatabase();

        var connection = _dbFixture.Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var customized = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(connection, npgsql => npgsql.UseNetTopologySuite());
                });

                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        });

        _client = customized.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static string UniqueEmail() => $"it_{Guid.NewGuid():N}@example.com";

    private static async Task<int> ReadIdAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task Register_ReturnsCreated_AndDto()
    {
        var payload = new
        {
            name = "Reg User",
            email = UniqueEmail(),
            password = "Secret123!",
        };

        var response = await _client.PostAsJsonAsync("/api/user/register", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(doc);
        Assert.True(doc!.RootElement.TryGetProperty("id", out _));
        Assert.Equal("Reg User", doc.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetUserById_Existing_ReturnsOk()
    {
        var email = UniqueEmail();
        var reg = await _client.PostAsJsonAsync(
            "/api/user/register",
            new
            {
                name = "User A",
                email,
                password = "Secret123!",
            }
        );
        reg.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(reg);

        var response = await _client.GetAsync($"/api/user/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(doc);
        var root = doc!.RootElement;
        Assert.Equal(id, root.GetProperty("id").GetInt32());
        Assert.Equal("User A", root.GetProperty("name").GetString());
        Assert.Equal(email, root.GetProperty("email").GetString());
    }

    [Fact]
    public async Task GetUserByEmail_Existing_ReturnsOk()
    {
        var email = UniqueEmail();
        var reg = await _client.PostAsJsonAsync(
            "/api/user/register",
            new
            {
                name = "User B",
                email,
                password = "Secret123!",
            }
        );
        reg.EnsureSuccessStatusCode();

        var response = await _client.GetAsync($"/api/user/email/{email}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(doc);
        var root = doc!.RootElement;
        Assert.Equal(email, root.GetProperty("email").GetString());
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithAtLeastOne()
    {
        await _client.PostAsJsonAsync(
            "/api/user/register",
            new
            {
                name = "List User",
                email = UniqueEmail(),
                password = "Secret123!",
            }
        );

        var response = await _client.GetAsync(
            "/api/user?pageNumber=1&pageSize=10&sortBy=Name&sortOrder=asc"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arrDoc = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(arrDoc);
        var arr = arrDoc!.RootElement;
        Assert.True(arr.ValueKind == JsonValueKind.Array);
        Assert.True(arr.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Create_Admin_ReturnsCreated()
    {
        var payload = new
        {
            name = "Admin Created",
            email = UniqueEmail(),
            password = "Secret123!",
            profile = Profile.MANAGER,
        };

        var response = await _client.PostAsJsonAsync("/api/user", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(doc);
        Assert.Equal("Admin Created", doc!.RootElement.GetProperty("name").GetString());
        Assert.True(doc.RootElement.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task Update_Admin_ReturnsOk_AndPersists()
    {
        var email = UniqueEmail();
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "To Update",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        var response = await _client.PutAsJsonAsync(
            $"/api/user/{id}",
            new
            {
                id,
                name = "Updated Name",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedDoc = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(updatedDoc);
        Assert.Equal("Updated Name", updatedDoc!.RootElement.GetProperty("name").GetString());

        var get = await _client.GetAsync($"/api/user/{id}");
        get.EnsureSuccessStatusCode();
        var gotDoc = await get.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(gotDoc);
        Assert.Equal("Updated Name", gotDoc!.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Delete_Admin_ReturnsOk_ThenNotFoundOnGet()
    {
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "To Delete",
                email = UniqueEmail(),
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        var del = await _client.DeleteAsync($"/api/user/{id}");
        Assert.Equal(HttpStatusCode.OK, del.StatusCode);

        var get = await _client.GetAsync($"/api/user/{id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = UniqueEmail();
        var payload = new
        {
            name = "Dup User",
            email,
            password = "Secret123!",
        };

        var first = await _client.PostAsJsonAsync("/api/user/register", payload);
        first.EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/user/register", payload);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidFields_ReturnsBadRequest()
    {
        var payload = new
        {
            name = "",
            email = "",
            password = "",
        };

        var resp = await _client.PostAsJsonAsync("/api/user/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task GetUserByEmail_NotFound_ReturnsNotFound()
    {
        var resp = await _client.GetAsync($"/api/user/email/{UniqueEmail()}");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Update_NullBody_ReturnsBadRequest()
    {
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "Null Body",
                email = UniqueEmail(),
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        var resp = await _client.PutAsync(
            $"/api/user/{id}",
            new StringContent("", Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_SortByName_Desc_Paging_Works()
    {
        var prefix = $"p_{Guid.NewGuid():N}".Substring(0, 8);

        var e1 = $"{prefix}_alpha@example.com";
        var e2 = $"{prefix}_beta@example.com";
        var e3 = $"{prefix}_zeta@example.com";

        await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = $"{prefix}_Alpha",
                email = e1,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = $"{prefix}_Beta",
                email = e2,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = $"{prefix}_Zeta",
                email = e3,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );

        var page1 = await _client.GetAsync(
            $"/api/user?pageNumber=1&pageSize=2&sortBy=Name&sortOrder=desc&filter={prefix}"
        );
        Assert.Equal(HttpStatusCode.OK, page1.StatusCode);
        var p1doc = await page1.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(p1doc);
        var arr1 = p1doc!.RootElement;
        Assert.Equal(2, arr1.GetArrayLength());
        Assert.Equal($"{prefix}_Zeta", arr1[0].GetProperty("name").GetString());
        Assert.Equal($"{prefix}_Beta", arr1[1].GetProperty("name").GetString());

        var page2 = await _client.GetAsync(
            $"/api/user?pageNumber=2&pageSize=2&sortBy=Name&sortOrder=desc&filter={prefix}"
        );
        Assert.Equal(HttpStatusCode.OK, page2.StatusCode);
        var p2doc = await page2.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(p2doc);
        var arr2 = p2doc!.RootElement;
        Assert.Equal(1, arr2.GetArrayLength());
        Assert.Equal($"{prefix}_Alpha", arr2[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task Update_ChangeProfile_Persists()
    {
        var email = UniqueEmail();
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "Prof User",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        var resp = await _client.PutAsJsonAsync(
            $"/api/user/{id}",
            new
            {
                id,
                name = "Prof User",
                email,
                password = "Secret123!",
                profile = Profile.MANAGER,
            }
        );
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var got = await _client.GetAsync($"/api/user/{id}");
        got.EnsureSuccessStatusCode();
        var gotDoc = await got.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(gotDoc);
        Assert.Equal("MANAGER", gotDoc!.RootElement.GetProperty("profile").GetString());
    }

    [Fact]
    public async Task Create_DuplicateEmail_ReturnsConflict()
    {
        var email = UniqueEmail();
        var body = new
        {
            name = "Dup Admin",
            email,
            password = "Secret123!",
            profile = Profile.CITIZEN,
        };

        var first = await _client.PostAsJsonAsync("/api/user", body);
        first.EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/user", body);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ReturnsUnauthorized()
    {
        using var unauth = _factory.CreateClient();
        var resp = await unauth.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "No Auth",
                email = UniqueEmail(),
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Register_TrimsAndSetsCitizenProfile()
    {
        var payload = new
        {
            name = "  Trim Name  ",
            email = $"  {UniqueEmail()}  ",
            password = "Secret123!",
        };

        var response = await _client.PostAsJsonAsync("/api/user/register", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOpts);
        Assert.NotNull(doc);
        var root = doc!.RootElement;
        Assert.Equal("  Trim Name  ", root.GetProperty("name").GetString());
        Assert.Equal("CITIZEN", root.GetProperty("profile").GetString());
    }

    [Fact]
    public async Task Register_SetsLocationHeader()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/user/register",
            new
            {
                name = "Loc User",
                email = UniqueEmail(),
                password = "Secret123!",
            }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var id = await ReadIdAsync(response);
        Assert.EndsWith($"/api/user/{id}", response.Headers.Location!.AbsolutePath);
    }

    [Fact]
    public async Task GetUserById_NonExisting_ReturnsNotFound()
    {
        var resp = await _client.GetAsync("/api/user/99999999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task GetUserById_InvalidId_ReturnsBadRequest()
    {
        var resp = await _client.GetAsync("/api/user/-1");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Update_NonExisting_ReturnsNotFound()
    {
        var email = UniqueEmail();
        var resp = await _client.PutAsJsonAsync(
            "/api/user/987654321",
            new
            {
                id = 987654321,
                name = "No One",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_IdZero_ReturnsBadRequest()
    {
        var resp = await _client.DeleteAsync("/api/user/0");
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task GetAll_InvalidPaging_ReturnsBadRequest()
    {
        var resp1 = await _client.GetAsync("/api/user?pageNumber=0&pageSize=10");
        Assert.Equal(HttpStatusCode.BadRequest, resp1.StatusCode);

        var resp2 = await _client.GetAsync("/api/user?pageNumber=1&pageSize=0");
        Assert.Equal(HttpStatusCode.BadRequest, resp2.StatusCode);
    }

    [Fact]
    public async Task Create_NullBody_ReturnsBadRequest()
    {
        var resp = await _client.PostAsync(
            "/api/user",
            new StringContent("", Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Create_SetsLocationHeader_AndProfileAsString_WithoutPassword()
    {
        var email = UniqueEmail();
        var resp = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "Hdr User",
                email,
                password = "Secret123!",
                profile = Profile.MANAGER,
            }
        );
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        Assert.NotNull(resp.Headers.Location);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(doc);

        var id = doc!.RootElement.GetProperty("id").GetInt32();
        Assert.EndsWith($"/api/user/{id}", resp.Headers.Location!.AbsolutePath);

        Assert.True(doc.RootElement.TryGetProperty("profile", out var prof));
        Assert.Equal("MANAGER", prof.GetString());
        Assert.False(doc.RootElement.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Register_Response_DoesNotContainPassword()
    {
        var resp = await _client.PostAsJsonAsync(
            "/api/user/register",
            new
            {
                name = "Safe User",
                email = UniqueEmail(),
                password = "Secret123!",
            }
        );
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(doc);
        Assert.False(doc!.RootElement.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task GetAll_Filter_NoMatches_ReturnsOkWithEmptyList()
    {
        var prefix = $"nohit_{Guid.NewGuid():N}";
        var resp = await _client.GetAsync(
            $"/api/user?pageNumber=1&pageSize=5&sortBy=Name&sortOrder=asc&filter={prefix}"
        );
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var list = await resp.Content.ReadFromJsonAsync<List<User>>(JsonOpts);
        Assert.NotNull(list);
        Assert.Empty(list!);
    }

    [Fact]
    public async Task Update_Unauthenticated_ReturnsUnauthorized()
    {
        var email = UniqueEmail();
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "NoAuth Upd",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        using var unauth = _factory.CreateClient();
        var resp = await unauth.PutAsJsonAsync(
            $"/api/user/{id}",
            new
            {
                id,
                name = "X",
                email,
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_Unauthenticated_ReturnsUnauthorized()
    {
        var created = await _client.PostAsJsonAsync(
            "/api/user",
            new
            {
                name = "NoAuth Del",
                email = UniqueEmail(),
                password = "Secret123!",
                profile = Profile.CITIZEN,
            }
        );
        created.EnsureSuccessStatusCode();
        var id = await ReadIdAsync(created);

        using var unauth = _factory.CreateClient();
        var resp = await unauth.DeleteAsync($"/api/user/{id}");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
