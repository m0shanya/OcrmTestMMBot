using Microsoft.AspNetCore.Mvc;

namespace OcrmTestMMBot.Controllers;

[ApiController]
[Route("[controller]")]
public class MmBotTestController : ControllerBase
{
    private readonly ILogger<MmBotTestController> _logger;
    private readonly string _source;
    private readonly List<string> _tokens;

    public MmBotTestController(ILogger<MmBotTestController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _source = configuration.GetValue<string>("BotData:Source");
        _tokens = configuration.GetSection("BotData:Tokens").Get<List<string>>();
    }

    [HttpPost("test")]
    public async Task<ActionResult<MmResponse>> Create()
    {
        StreamReader reader = new StreamReader(HttpContext.Request.Body);
        string body = await reader.ReadToEndAsync();

        _logger.LogInformation(System.DateTime.Now + " " + body);

        var keyValues = new Dictionary<string, string> { };
        foreach (var st in body.Replace(" ", "").Split("&"))
        {
            var temp = st.Split("=");
            keyValues[temp[0]] = temp[1];
        }

        try
        {
            if (!_tokens.Contains(keyValues["token"]))
            {
                var error = $"Invalid token {keyValues["token"]}";
                _logger.LogError(error);
                return Unauthorized(error);
            }

            var test = Test.Instance(_logger, keyValues["channel_id"]);

            if (!Test.isRunning)
            {
                _ = Task.Run(async () => await test.TestAddresses(_source));
            }

            var response = new MmResponse().GetResponse(keyValues["user_name"], Test.isRunning);
            _logger.LogInformation($"[{System.DateTime.Now}] {response}");

            return response;
        }
        catch (Exception e)
        {
            _logger.LogError($"[{System.DateTime.Now}] {e.Message}");

            return BadRequest("Invalid request body");
        }
    }
}
