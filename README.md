DotChat
===

### Etap 1
Znajduje się na branchu `etap-1`</br>
Raport z wykonania projektu: [etap-1.md](doc-1/etap-1.md)

### Etap 2
Znajduje się na branchu `etap-2`</br>
Raport z wykonania projektu: [etap-2.md](doc-2/etap-2.md)

### Etap 3
Znajduje się na branchu `etap-3`</br>
Raport z wykonania projektu: [etap-3.md](doc-3/etap-3.md)

### Etap 4
Znajduje się na branchu `etap-4`</br>
Raport z wykonania projektu: [etap-4.md](doc-4/etap-4.md)

### Opis 
`temat 13` Tworzenie interfejsu chatbotowego w C#<br>
Zadanie polega na stworzeniu interfejsu chatbota w C# korzystjącego z API OpenAI. Program powinien pozwalać na prowadzenie wieloetapowej rozmowy z użytkownikiem w _konsoli_.

`temat 4` Tworzenie streszczenia treści dokumentu PDF </br>
   **Opis zadania:**  
   - Korzystając z OpenAI API (np. GPT-4), załaduj plik PDF, a następnie prześlij jego zawartość do modelu, aby wygenerował streszczenie.  
   - Wygenerowane streszczenie zapisz w pliku i wyświetl w konsoli.  
   - Program powinien mieć możliwość wygenerowania streszczeń wielu plików umieszczonych w folderze 

`temat 7` Generowanie obrazów na podstawie opisu </br>
   **Opis zadania:**  
   - Wykorzystując API OpenAI i model DALL-E, stwórz aplikację generującą obraz na podstawie dostarczonego przez użytkownika opisu.

`temat 3` Transkrypcja filmu z YouTube z tłumaczeniem </br>
   **Opis zadania:**  
   - Stwórz aplikację, która:
     1. Pobierze napisy do filmu z YouTube 
     2. Przetłumaczy transkrypcję na wybrany język (np. angielski na polski) przy pomocy OpenAI ChatGPT.  
   - Wynikowy tekst zapisz w formacie `.docx`.
   - Program powinien mięc mozliwość wygenerowania streszczenia 

###### Jak korzystać
```bash
dotnet build
dotnet run
```
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
