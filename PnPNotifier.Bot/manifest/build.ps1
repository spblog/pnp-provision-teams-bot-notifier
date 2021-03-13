param ([string]$target, [string]$projectDir, [string]$targetDir);

if($target -eq "Debug"){
    Compress-Archive -Path "${projectDir}manifest\dev\*\" -DestinationPath "${targetDir}package-dev.zip" -Force
}

if($target -eq "Release"){
    Compress-Archive -Path "${projectDir}manifest\prod\*\" -DestinationPath "${targetDir}package-prod.zip" -Force
}