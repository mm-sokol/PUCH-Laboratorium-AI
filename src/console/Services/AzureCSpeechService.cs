using System.Collections;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Configuration;


namespace AzureCustomSpeech
{
  enum Language
  {
    Polish, Icelandic, English, Italian, None
  }

  class LanguageDescription {
    public static string Get(Language lang) {
      return lang switch {
        Language.Polish => "pl-PL",
        Language.Icelandic => "is-IS",
        Language.English => "eng-GB",
        Language.Italian => "it-IT",
        Language.None => "",
        _ => "unkonwn"
      };
    }
  }
  class AzureCSpeechService {
    private readonly string _apiKey;
    private readonly string _endpoint;

    private readonly string _region;

    private readonly SpeechTranslationConfig _speechTranslationConfig;
    private readonly AudioConfig _audioConfig;

    public AzureCSpeechService(IConfiguration configuration)
    {
      _apiKey = configuration["AzureCustomSpeech:ApiKey"] ?? "";
      _endpoint = configuration["AzureCustomSpeech:Endpoint"] ?? "";
      _region = configuration["AzureCustomSpeech:Region"] ?? "";
      _speechTranslationConfig = SpeechTranslationConfig.FromSubscription(_apiKey, _region);




    }

    public async Task AudioToText(Language srcLang, Language tgtLang) {

      _speechTranslationConfig.SpeechRecognitionLanguage = LanguageDescription.Get(srcLang);
      _speechTranslationConfig.AddTargetLanguage(LanguageDescription.Get(tgtLang));

      using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
      using var recogniser = new TranslationRecognizer(_speechTranslationConfig, audioConfig);

      var result = await recogniser.RecognizeOnceAsync();
    }

    public async Task TextToAudio(Language srcLang, Language tgtLang) {

      _speechTranslationConfig.SpeechSynthesisLanguage = LanguageDescription.Get(srcLang);
      _speechTranslationConfig.SpeechSynthesisVoiceName = 
    }
  }

  
}