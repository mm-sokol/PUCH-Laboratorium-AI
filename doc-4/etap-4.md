DotChat
===
`etap-4`

1. [Custom Speech](#1-custom-speech)
2. [temat 3: Transkrypcja filmu z YouTube z tłumaczeniem](#2-transkrypcja-filmu-z-youtube-z-tłumaczeniem)

Repozytorium: [github](https://github.com/mm-sokol/PUCH-Laboratorium-AI/tree/etap-4)

```yaml
:--------------------------------------------------------:
               D O T  C H A T  gpt-4      
:--------------------------------------------------------:
 Here are some usefull commands:
 \user <username> - to register your username
 \system <text> - to provide context for the AI assistant
 \save <filename> - to save your chat history in a file
 \clear - to clear the chat history
 \exit - for leaving the chat

 \vision [options] - predicts weather from given image with Azure Custom Vision
 \vision img "<path to img>"
 \vision url "<url with img>"

 \summarize [options] - creates summaries of pdf files with OpenAI
 \summarize pdf "<in filename>" to "<out filename>"
 \summarize dir "<source path>" to "dest path>"
 \summarize ... -v|--verbose - outputs summary to screen

 \receipt [options] - extract data from receipt image using Azure Custom Document Intelligence
 \receipt jpg "<in filename>" to json "<out filename>"
 \receipt ... -v|--verbose - outputs data to the screen

 \dall-e [options] - prompts for image description and generates image(s)
 \dall-e img "<dest folder>" - images will be saved to disc
 \dall-e url - image urls will be printed to console

 \speech <filename> - [in the making...]
 \youtube - [int the making...]

 ...
```

## 1. Custom Speech
W module korzystającym z `Custom Speech` celem będzie wykonanie 
#### a. Założenie zasobu `Speech services`

<table>
    <tr>
        <th>Portal -> </th>
        <th>Stworzenie nowego zasobu -> </th>
        <th>Konfiguracja Speech services</th>
    </tr>
    <tr>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\1-speech-service.png" width="100">
        </div></td>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\2-create.png" height="120">
        </div></td>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\3-create-details.png" height="120">
        </div></td>
    </tr>
</table>

<table>
    <tr>
        <th>Potwierdzenie parametrów -> </th>
        <th>Ekran utworzonego zasobu -> </th>
        <th>Odnalezienie kluczy API</th>
    </tr>
    <tr>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\4-create-review.png" height="120">
        </div></td>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\5-creation-complete.png" height="120">
        </div></td>
        <td><div style="text-align: center;">
        <img src="screens\speech\1-setup\6-service-keys.png" height="120">
        </div></td>
    </tr>
</table>

#### b. Dodanie klucza api i endpointu do pliku konfiguracji
```json
// appsettings.json
// ...
  "AzureCustomSpeech":{
    "Endpoint": "<Endpoint>",
    "ApiKey": "<KEY 1>",
    "Region": "westeurope"
  }
}
```
#### b. Dodanie zależności Cognitive Speech SDK
```bash
dotnet add package Microsoft.CognitiveServices.Speech
```

#### d. 
#### e. 
#### f. 
#### g. 
#### h. 

## 2. Transkrypcja filmu z YouTube z tłumaczeniem