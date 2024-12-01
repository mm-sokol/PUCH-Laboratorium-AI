# Laboriatorium 2
---
`temat 13` Tworzenie interfejsu chatbotowego w C#<br>
Zadanie polega na stworzeniu interfejsu chatbota w C# korzystjącego z API OpenAI. Program powinien pozwalać na prowadzenie wieloetapowej rozmowy z użytkownikiem w _konsoli_.



### Podjęte kroki:
#### 1. Utworzenie konta OpenAI i zasilenie go dla uzyskania możliwości odpytania API modelu gpt

<div style="display: flex; gap: 20px;">
  <div style="flex: 1; text-align: center;">
<img src="screens/credit-balance.png" alt="Credit balance" width="400" />
<img src="screens/create-api.png" alt="Crete API Key" width="400" />
  </div>
  <div style="flex: 1; text-align: center;">
<img src="screens/create-key-form.png" alt="Crete API Key" width="400" />
  </div>
</div>

#### 2. Utworzenie zasobu Azure OpenAI
- Zainstalownie rozszerzenia Azure Intelij Community: <br>
```Ctrl+Shift+X``` - otwiera panel z rozszerzeniami
![Azure Toolkit](screens/azure-toolkit.png)
- Wejście w panel Azure Explorer po prawej stronie ekranu
- Zalogowanie się do konta Azure: kliknięcie prawym klawiszem na`Azure`  i wybranie z listy `Sign in`

| Azure Explorer      |
|:------------------:|
| ![alt text](screens/intellij-azure.png)   |


- Wybranie trybu logowanie `Azure CLI`

| Opcje logowania      |
|:----------------:|
| ![alt text](screens/intellij-azure-login.png)     |

- Utworzenie usługi `OpenAI`

<div style="display: flex; gap: 20px;">
  <div style="flex: 1; text-align: center;">
<img src="screens/intellij-create-service-1.png" alt="Credit balance" width="400" />
  </div>
  <div style="flex: 1; text-align: center;">
<img src="screens/intellij-create-form.png" alt="Crete API Key" width="5000" />
  </div>
</div>

- Utworzenie wdrożenia `deployment` 
<div style="display: flex; gap: 20px;">
  <div style="flex: 1; text-align: center;">
<img src="screens/intellij-create-deployment-2.png" alt="Credit balance" width="400" />
  </div>
  <div style="flex: 1; text-align: center;">
<img src="screens/intellij-gpt-35-deployment.png" alt="Crete API Key" width="5000" />
  </div>
</div>


- Odnalezienie grupy zasobów w Portalu Azure
- Wybór `ChatDotAI-service`

![alt text](screens/ui-resource-group.png)
- Wybór `Azure OpenAI Studio`

![alt text](screens/ui-openai-sturio.png)

- Przejście do `deployments` (panel po prawej stronie)

![alt text](screens/ui-deployments-panel.png)

- Wybór typu wdrożenia: [deployment types](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/deployment-types#global-standard)
- Podównanie cen dla poszczególnych modeli: [](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/)
- Sprawdzenie dostępnych modeli: `quota`
- Wybór `deploy base model`

![alt text](screens/ui-deploy-base-model.png)

- Wybór modelu `gpt-4`

![alt text](screens/ui-gpt-4-model.png)

![alt text](screens/ui-gpt-4-deployment.png)

- Wdrożenia widoczne są w Azure Explorer

![alt text](screens/intellij-deployments-explorer.png)


#### 3. Utworzenie aplikacji
- Utworzenie szkieletu aplikacji konsolowej
```bash
dotnet new console -m console
```

- Dodanie zależności 
```bash
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Azure.AI.OpenAI
dotnet add package OpenAI-API-dotnet
```

- Utworzenie klasy `OpenAIService` <br>
Klasa ma za zadanie utworzyć klienta http komunikującego się z wybranym modelem w Azure OpenAI. <br>
Początkowo plan obejmował wykrorzystanie Azure.AI.OpenAI, jendak pakiet nie był rozpoznawany pomimo dodania zależnoścji:
```bash
error CS0246: Nie można znaleźć nazwy typu lub przestrzeni nazw „OpenAIClient”
error CS0246: Nie można znaleźć nazwy typu lub przestrzeni nazw „ChatCompletionsClient”
```
Problem został rozwiązany przez utworzenie klienta http:
```C#
// plik OpenAIService.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
```

- Dodanie niezbędnych funkcjonalności klasy `OpenAIService`
Klasa 
```C#
    private readonly string _apiKey;
    private readonly string _endpoint;
    private string _model {get; set;}
    private List<Tuple<Agent, string>> _conversationHistory;
    private HttpClient _httpClient;
```
### Rezultaty: