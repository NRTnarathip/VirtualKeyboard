
set AppName=abc.smapi.gameloader

adb shell am force-stop %AppName%

if errorlevel 1 (
    echo ADB is not connected.
    exit /b
)

adb shell mkdir  "/storage/emulated/0/Android/data/%AppName%/files/Mods/VirtualKeyboard"
adb push "bin/Release/net8.0/VirtualKeyboard.dll" "/storage/emulated/0/Android/data/%AppName%/files/Mods/VirtualKeyboard"
adb push "bin/Release/net8.0/manifest.json" "/storage/emulated/0/Android/data/%AppName%/files/Mods/VirtualKeyboard"

adb shell am start %AppName%"/crc64e91f1276c636690c.LauncherActivity"

