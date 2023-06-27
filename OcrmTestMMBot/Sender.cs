using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using OcrmTestMMBot.Controllers;

namespace OcrmTestMMBot;
public class Sender
{
    private string _token { get; set; }
    private string _url { get; set; }
    private string _chatID { get; set; }
    private HttpClient _client { get; set; }
    private readonly ILogger<MmBotTestController> _logger;

    public Sender(ILogger<MmBotTestController> logger, string chatID)
    {
        _logger = logger;
        _chatID = new String(Environment.GetEnvironmentVariable("MM_CHATID") is null ? chatID : Environment.GetEnvironmentVariable("MM_CHATID"));
        _url = new String(Environment.GetEnvironmentVariable("MM_URL") is null ? "https://mm.mts.by/api/v4/" : Environment.GetEnvironmentVariable("MM_URL"));
        _token = new String(Environment.GetEnvironmentVariable("MM_TOKEN") is null ? "mfjbmf8suinoip6inowqxhjbpo" : Environment.GetEnvironmentVariable("MM_TOKEN"));

        HttpClientHandler _handler = new()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
        {
            return true;
        }
        };
        _client = new(_handler);
    }

    private async Task<string> UploadImage(byte[] image)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(_url + "files"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        MultipartFormDataContent form = new MultipartFormDataContent();
        form.Add(new StringContent(_chatID), "channel_id");
        form.Add(new ByteArrayContent(image, 0, image.Length), "files", "file.png");
        request.Content = form;

        HttpResponseMessage response = await Task.Run(() => _client.SendAsync(request).Result);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation($"[{DateTime.Now}] Image upload");
        string sd = response.Content.ReadAsStringAsync().Result;
        dynamic entity = JsonConvert.DeserializeObject(sd)!;

        return entity.file_infos[0].id;
    }

    public async Task SendMessage(string message, byte[] image)
    {
        message = $"[{DateTime.Now}] " + message;
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _url + "posts");
        requestMessage.Headers.Add("Authorization", $"Bearer {_token}");

        string imageId = await UploadImage(image);
        
        var reqData = new
        {
            channel_id = _chatID,
            message = message,
            file_ids = new[] { imageId }
        };
        requestMessage.Content = new StringContent(JsonConvert.SerializeObject(reqData), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.SendAsync(requestMessage);
        _logger.LogInformation($"[{DateTime.Now}] Message send");
        var body = await response.Content.ReadAsStringAsync();
        int i = (int)response.StatusCode;
        var code = i.ToString();
    }
}