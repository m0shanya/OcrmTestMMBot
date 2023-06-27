># OCRMTestMmBot
>***This project is about testing by the WebAPI. Results are sending into the Mattermost bot which lies on server.***
>
>## If you need to use this project: in appsettings.json file you can change
>```
> 1. Source
> 2. Tokens 
> 3. SendToken  
>in BotData
>```
>### Usage
>```
> - AspNetCore, Version=6.2.3
> - Selenium.WebDriver, Version=4.10.0
> - Newtonsoft.Json, Version=13.0.3
> - SeleniumExtras.WaitHelpers, Version=1.0.2
>```

### How to run container
```shell
docker run --restart unless-stopped --name "ocrmtestmmbot" -idt -p 5031:80 dockerhub.inside.mts.by/malahov/ocrmtestmmbot
```