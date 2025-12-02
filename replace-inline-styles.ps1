# Script to replace common inline styles with Tailwind classes in Razor files

$rootPath = "c:\Users\alhab\RiderProjects\StudentResidenceManagementSystemSRMS\SRMS.WebUI.Server\Components\Pages"

# Define replacements
$replacements = @(
    @{ Pattern = 'Style="text-align: right"'; Replacement = 'Class="text-right"' }
    @{ Pattern = 'Style="text-align: center"'; Replacement = 'Class="text-center"' }
    @{ Pattern = 'Style="text-align: left"'; Replacement = 'Class="text-left"' }
    @{ Pattern = 'Style="font-weight: bold;"'; Replacement = 'Class="font-bold"' }
    @{ Pattern = 'Style="font-weight: 600;"'; Replacement = 'Class="font-semibold"' }
    @{ Pattern = 'Style="font-weight: 700;"'; Replacement = 'Class="font-bold"' }
    @{ Pattern = 'Style="max-width: 300px;"'; Replacement = 'Class="max-w-[300px]"' }
    @{ Pattern = 'Style="max-width: 400px;"'; Replacement = 'Class="max-w-[400px]"' }
    @{ Pattern = 'Class="pa-4"'; Replacement = 'Class="p-4"' }
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

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    foreach ($replacement in $replacements) {
        $content = $content -replace [regex]::Escape($replacement.Pattern), $replacement.Replacement
    }
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $totalChanges++
        Write-Host "Updated: $($file.FullName)" -ForegroundColor Green
    }
}

Write-Host "`nTotal files updated: $totalChanges" -ForegroundColor Cyan
