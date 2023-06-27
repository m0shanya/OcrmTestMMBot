namespace OcrmTestMMBot;
public class MmResponse
{
    public string Text {get;set;}
    public string Response_Type {get;set;}
    public MmResponse GetResponse(string name, bool isRunning)
    {
        var message = $"Здравствуйте, {name}. Вы меня просили протестировать. " +
        (isRunning ? "Ожидайте завершения предыдущего тестирования." : "Тестирование принято в обработку.");

        var request = new MmResponse{Text = message, Response_Type = "ephemeral"};

        return request;
    }
}