# Install-FFmpeg.ps1
$ffmpegPath = "C:\ffmpeg\bin\ffmpeg.exe"

if (-Not (Test-Path $ffmpegPath)) {
    Write-Output "FFmpeg not found. Installing FFmpeg..."

    $ffmpegUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
    $ffmpegZipPath = "C:\ffmpeg.zip"
    $ffmpegExtractPath = "C:\ffmpeg"

    Invoke-WebRequest -Uri $ffmpegUrl -OutFile $ffmpegZipPath
    Expand-Archive -Path $ffmpegZipPath -DestinationPath $ffmpegExtractPath

    # Move the extracted files to the desired location
    Move-Item -Path "$ffmpegExtractPath\ffmpeg-*-essentials_build\*" -Destination $ffmpegExtractPath

    # Clean up
    Remove-Item -Path $ffmpegZipPath
    Remove-Item -Path "$ffmpegExtractPath\ffmpeg-*-essentials_build" -Recurse

    Write-Output "FFmpeg installed successfully."
} else {
    Write-Output "FFmpeg is already installed."
}
