# Logowanie do Azure
Connect-AzAccount

# Ustaw zmienne
$resourceGroupName = "PUCH-ChatGroup"
$location = "West Europe"  # Możesz zmienić na inną lokalizację
$accountName = "DotChat-Vision"

# Utwórz grupę zasobów (jeśli nie istnieje)
# New-AzResourceGroup -Name $resourceGroupName -Location $location

# Utwórz zasób Custom Vision Training
New-AzCognitiveServicesAccount `
    -ResourceGroupName $resourceGroupName `
    -Name $trainingAccountName `
    -Type CustomVision.Training `
    -SkuName S0 `
    -Location $location

# Utwórz zasób Custom Vision Prediction
New-AzCognitiveServicesAccount `
    -ResourceGroupName $resourceGroupName `
    -Name $predictionAccountName `
    -Type CustomVision.Prediction `
    -SkuName S0 `
    -Location $location

# Pobierz klucze dostępu dla Training
$trainingKeys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -Name $trainingAccountName

# Pobierz klucze dostępu dla Prediction
$predictionKeys = Get-AzCognitiveServicesAccountKey -ResourceGroupName $resourceGroupName -Name $predictionAccountName

# Wyświetl klucze
Write-Host "Training Key 1: $($trainingKeys.Key1)"
Write-Host "Training Key 2: $($trainingKeys.Key2)"

Write-Host "Prediction Key 1: $($predictionKeys.Key1)"
Write-Host "Prediction Key 2: $($predictionKeys.Key2)"