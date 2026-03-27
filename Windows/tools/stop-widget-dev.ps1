# Cierra procesos del proyecto y, si se indica -PackageRoot, intenta borrar resources.pri bloqueados (DEP1000 / 0x800704C8).
# Uso desde MSBuild: se pasa -PackageRoot "$(MSBuildProjectDirectory)\bin\$(Platform)\$(Configuration)"
param(
    [string] $PackageRoot = ''
)

$ErrorActionPreference = 'SilentlyContinue'

$names = @(
    'Widget.TimeTracking.WidgetHost',
    'Widget.TimeTracking.App'
)

foreach ($n in $names) {
    Get-Process -Name $n -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Finalizando $($_.ProcessName) (PID $($_.Id))..."
        Stop-Process -Id $_.Id -Force
    }
}

foreach ($exe in @('Widget.TimeTracking.WidgetHost.exe', 'Widget.TimeTracking.App.exe')) {
    & taskkill /F /IM $exe 2>$null | Out-Null
}

# A veces el panel de widgets mantiene handles; cerrar solo si el usuario lo permite (opcional).
if ($env:STOP_WIDGETS_EXE -eq '1') {
    & taskkill /F /IM Widgets.exe 2>$null | Out-Null
}

if ($PackageRoot -and (Test-Path -LiteralPath $PackageRoot)) {
    Start-Sleep -Milliseconds 400
    $candidates = @(
        (Join-Path $PackageRoot 'resources.pri'),
        (Join-Path $PackageRoot 'AppX\resources.pri')
    )
    foreach ($p in $candidates) {
        if (Test-Path -LiteralPath $p) {
            Remove-Item -LiteralPath $p -Force -ErrorAction SilentlyContinue
            if (Test-Path -LiteralPath $p) {
                Write-Host "No se pudo borrar (bloqueado): $p"
            } else {
                Write-Host "Eliminado: $p"
            }
        }
    }
}

Write-Host "Listo. Si sigue DEP1000: Shift+F5, cierra VS, borra bin/obj del Package o ejecuta con `$env:STOP_WIDGETS_EXE=1"
