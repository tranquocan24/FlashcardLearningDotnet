# Simple emoji removal script
$files = Get-ChildItem -Path "." -Include "*.html","*.js","*.md" -Recurse

foreach($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    
    # Remove common emojis
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '?', '+'
    $content = $content -replace '??', ''
    $content = $content -replace '???', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '?', ''
    $content = $content -replace '?', ''
    $content = $content -replace '?', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '?', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '???', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '???', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', 'NEW'
    $content = $content -replace '??', 'UP'
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    $content = $content -replace '??', ''
    
    if($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host "Done! All emojis removed."
