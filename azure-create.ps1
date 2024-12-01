az cognitiveservices account create `
  --name "ChatDotAI-resource" `
  --resource-group "PUCH-ChatGroup" `
  --kind OpenAI `
  --sku S0 `
  --location "West Europe"


az openai deployment create `
  --resource-group PUCH-ChatGroup `
  --resource-name  ChatDotAI-service`
  --deployment-name gpt35-deployment `
  --model-name gpt35 `
  --scale-type "Standard"

