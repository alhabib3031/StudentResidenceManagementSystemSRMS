# Enhanced script to replace all remaining inline styles with Tailwind classes

$rootPath = "c:\Users\alhab\RiderProjects\StudentResidenceManagementSystemSRMS\SRMS.WebUI.Server\Components\Pages"

# Define comprehensive replacements
$replacements = @(
    # Width and height
    @{ Pattern = 'Style="width: 100%"'; Replacement = 'Class="w-full"' }
    @{ Pattern = 'Style="width: 100%;"'; Replacement = 'Class="w-full"' }
    @{ Pattern = 'Style="height: 100%"'; Replacement = 'Class="h-full"' }
    @{ Pattern = 'Style="height: 100%;"'; Replacement = 'Class="h-full"' }
    @{ Pattern = 'Style="height: 400px;"'; Replacement = 'Class="h-[400px]"' }
    @{ Pattern = 'Style="height: 300px;"'; Replacement = 'Class="h-[300px]"' }
    @{ Pattern = 'Style="max-height: 400px; overflow-y: auto;"'; Replacement = 'Class="max-h-[400px] overflow-y-auto"' }
    @{ Pattern = 'Style="max-height: 300px; overflow-y: auto;"'; Replacement = 'Class="max-h-[300px] overflow-y-auto"' }
    
    # Font sizes
    @{ Pattern = 'Style="font-size: 80px;"'; Replacement = 'Class="text-[80px]"' }
    @{ Pattern = 'Style="font-size: 72px;"'; Replacement = 'Class="text-[72px]"' }
    @{ Pattern = 'Style="font-size: 64px;"'; Replacement = 'Class="text-[64px]"' }
    @{ Pattern = 'Style="font-size: 48px;"'; Replacement = 'Class="text-5xl"' }
    
    # Font weights
    @{ Pattern = 'Style="font-weight: 800;"'; Replacement = 'Class="font-extrabold"' }
    @{ Pattern = 'Style="font-weight: 700;"'; Replacement = 'Class="font-bold"' }
    @{ Pattern = 'Style="font-weight: 600;"'; Replacement = 'Class="font-semibold"' }
    @{ Pattern = 'Style="font-weight: 500;"'; Replacement = 'Class="font-medium"' }
    @{ Pattern = 'Style="font-weight: bold;"'; Replacement = 'Class="font-bold"' }
    
    # Text alignment
    @{ Pattern = 'Style="text-align: right"'; Replacement = 'Class="text-right"' }
    @{ Pattern = 'Style="text-align: center"'; Replacement = 'Class="text-center"' }
    @{ Pattern = 'Style="text-align: left"'; Replacement = 'Class="text-left"' }
    
    # Max width
    @{ Pattern = 'Style="max-width: 400px; overflow: hidden; text-overflow: ellipsis;"'; Replacement = 'Class="max-w-[400px] overflow-hidden text-ellipsis"' }
    @{ Pattern = 'Style="max-width: 300px; overflow: hidden; text-overflow: ellipsis;"'; Replacement = 'Class="max-w-[300px] overflow-hidden text-ellipsis"' }
    @{ Pattern = 'Style="max-width: 300px;"'; Replacement = 'Class="max-w-[300px]"' }
    @{ Pattern = 'Style="max-width: 400px;"'; Replacement = 'Class="max-w-[400px]"' }
    
    # Flex
    @{ Pattern = 'style="flex: 1;"'; Replacement = 'class="flex-1"' }
    
    # Font family
    @{ Pattern = 'Style="font-family: monospace;"'; Replacement = 'Class="font-mono"' }
    
    # MudBlazor to Tailwind
    @{ Pattern = 'Class="pa-4"'; Replacement = 'Class="p-4"' }
    @{ Pattern = 'Class="pa-5"'; Replacement = 'Class="p-5"' }
    @{ Pattern = 'Class="pa-6"'; Replacement = 'Class="p-6"' }
    @{ Pattern = 'Class="pa-8"'; Replacement = 'Class="p-8"' }
    @{ Pattern = 'Class="d-flex'; Replacement = 'Class="flex' }
    @{ Pattern = 'class="d-flex'; Replacement = 'class="flex' }
    @{ Pattern = 'justify-space-between'; Replacement = 'justify-between' }
    @{ Pattern = 'align-center'; Replacement = 'items-center' }
)

# Get all .razor files
$files = Get-ChildItem -Path $rootPath -Filter "*.razor" -Recurse

$totalChanges = 0
$filesChanged = @()

foreach ($file in $files) {
    try {
        $content = Get-Content $file.FullName -Raw -ErrorAction Stop
        $originalContent = $content
        
        foreach ($replacement in $replacements) {
            $content = $content -replace [regex]::Escape($replacement.Pattern), $replacement.Replacement
        }
        
        if ($content -ne $originalContent) {
            Set-Content -Path $file.FullName -Value $content -NoNewline -ErrorAction Stop
            $totalChanges++
            $filesChanged += $file.Name
            Write-Host "✅ Updated: $($file.Name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "❌ Error processing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total files updated: $totalChanges" -ForegroundColor Yellow
if ($filesChanged.Count -gt 0) {
    Write-Host "`nFiles changed:" -ForegroundColor Cyan
    $filesChanged | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
}
Write-Host "═══════════════════════════════════════`n" -ForegroundColor Cyan
