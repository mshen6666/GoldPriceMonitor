@echo off
setlocal
cd /d "%~dp0"
chcp 65001 >nul
echo ============================================
echo          金价浮窗监控器 - GoldPriceMonitor
echo ============================================
echo.

dotnet --version >nul 2>&1
if errorlevel 1 goto :no_sdk

echo [1/2] 正在构建项目...
dotnet build GoldPriceMonitor.csproj -c Release -o release
if errorlevel 1 goto :build_failed

if not exist "release\GoldPriceMonitor.exe" goto :exe_missing

echo [2/2] 正在启动金价监控器...
echo.
echo ============================================
echo  程序已启动！
echo  - 右键任务栏图标可操作菜单
echo ============================================
echo.

start "" /D "%~dp0release" "GoldPriceMonitor.exe"

REM 等待程序启动
timeout /t 2 /nobreak >nul

REM 检查程序是否成功启动（通过检查进程是否存在）
tasklist /FI "IMAGENAME eq GoldPriceMonitor.exe" 2>nul | find /I /N "GoldPriceMonitor.exe" >nul
if errorlevel 1 goto :not_running

echo [成功] 金价监控器已成功启动并运行！
goto :end

:no_sdk
echo [错误] 未检测到 .NET SDK，请先安装 .NET 10.0
echo 下载地址: https://dotnet.microsoft.com/download/dotnet/10.0
goto :pause_end

:build_failed
echo [错误] 项目构建失败
goto :pause_end

:exe_missing
echo [错误] 未找到 release\GoldPriceMonitor.exe
goto :pause_end

:not_running
echo [警告] 未能确认程序是否启动成功，请检查系统托盘
goto :pause_end

:pause_end
echo.
pause
goto :eof

:end
echo.
pause
