DotChat
===
`etap-3`
1. [Custom Document](#1-custom-document)
  - [Storage Account](#4-tworzenie-storage-account)
  - [Blob Storage Container](#5-dodanie-kontenera-do-storage-account)
  - [Projekt w Document Intelligence Studio](#7-utworzenie-projektu-w-document-intelligence-studio)
  - [Trenowanie modelu]()
  - [Integracja z czatem]()

2. [temat 7: Generowanie obrazów na podstawie opisu](#2-generowanie-obrazów-na-podstawie-opisu)

Repozytorium: [github](https://github.com/mm-sokol/PUCH-Laboratorium-AI/tree/etap-3)

Crediting data sources:
- [ExpressExpense.com](https://expressexpense.com/blog/free-receipt-images-ocr-machine-learning-dataset/)
- [OCR Receipts from Grocery Stores Text Detection - kaggle dataset](https://www.kaggle.com/datasets/trainingdatapro/ocr-receipts-text-detection)

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

  \vision [options] - predicts weather from 
  given image with Azure Custom Vision
  \vision img "<path to img>"
  \vision url "<url with img>"

  \summarize [options] - creates summaries of pdf files with OpenAI
  \summarize pdf "<in filename>" to "<out filename>"
  \summarize dir "<source path>" to "<dest path>"
  \summarize ... -v|--verbose - outputs summary to screen

  \doc <filename> - [in the making]
  \img <description> - [in the making]
  ...
```

### 1. Custom Document

1. Ze względu na to, że w poprzednim etapie został już utworzony zasób Document Intelligence ten krok został tu pominięty
2. Przejście do `Document Intelligence Studio`

![alt text](screens/1_doc_intelligence/3_doc_studio.png)

3. Wybranie `Custom extractoin model` na dole strony

![text](screens/1_doc_intelligence/4_cutom_extraction.png)


Ponieważ projekt wymaga użycia `Blob Storage Container` został utworzony `Storatge Account`

#### 4. Tworzenie `Storage Account`

![text](screens/1_doc_intelligence/7_storage_account.png)

![text](screens/1_doc_intelligence/8_create_storage.png)

![alt text](screens/1_doc_intelligence/9_create_blob_storage_1.png)

![alt text](screens/1_doc_intelligence/10_hot_cold.png)

![alt text](screens/1_doc_intelligence/11_created_storage_account.png)

#### 5. Dodanie kontenera do `Storage Account`

![alt text](screens/1_doc_intelligence/12_containers.png)

![alt text](screens/1_doc_intelligence/13_new_container.png)

![alt text](screens/1_doc_intelligence/14_naming_container.png)

6. Dodanie danych traningowych do kontenera

![text](screens/1_doc_intelligence/15_upload_icon.png)

![alt text](screens/1_doc_intelligence/16_upload_files.png)

#### 7. Utworzenie projektu w `Document Intelligence Studio`

![alt text](screens/1_doc_intelligence/5_create_project.png)

*krok 1 w konfiguracji projektu*

![alt text](screens/1_doc_intelligence/6_project_details.png)

*krok 2 w konfiguracji projektu*

![alt text](screens/1_doc_intelligence/17_configure_service_resource.png)

*krok 3 konfiguracji*

![alt text](screens/1_doc_intelligence/18_connect_to_data_source.png)

#### 8. Trenowanie modelu

- etykietowanie danych automatyczne

![alt text](screens/1_doc_training/1_auto_label.png)

![alt text](screens/1_doc_training/2_prebuilt_receipt.png)

*nie było możliwe zaetykietowanie wszystkich obrazów*

![alt text](screens/1_doc_training/3_error.png)

*obrazy były etykietowane pojedyńczo*

*okazjonalnie występowały konfilkty etykiet*

![alt text](screens/1_doc_training/4_review_labels.png)

*rozwiązywane przez usuwanie jednej z nakładających się etykiet*

![alt text](screens/1_doc_training/5_delete_label.png)



<table>
  <tr>
    <th colspan="2">Wyniki etykietowania</th>
    <th colspan="1">Przykładowe Etykiety</th>
  </tr>
  <tr>
    <td>
      <div style="text-align: center;">
        <img src="screens/1_doc_training/6_labeled_receipt.png" alt="text" width="234"/>
      </div>
    </td>
    <td>
      <div style="text-align: center;">
        <img src="screens/1_doc_training/7_labeled_receipt.png" alt="text" width="200"/>
      </div>
    </td>
    <td>
      <div style="text-align: center;">
        <img src="screens/1_doc_training/8_labels.png" alt="text" width="160"/>
      </div>
    </td>
  </tr>
</table>




### 2. Generowanie obrazów na podstawie opisu