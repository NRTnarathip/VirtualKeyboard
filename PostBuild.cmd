echo my current folder excute %cd%
adb shell am force-stop com.chucklefish.stardewvalley

if errorlevel 1 (
    echo ADB is not connected.
    exit /b
)

 adb shell mkdir  "/storage/emulated/0/Android/data/com.chucklefish.stardewvalley/files/Mods/VirtualKeyboard"
 adb push "bin/Release/net5.0/VirtualKeyboard.dll" "/storage/emulated/0/Android/data/com.chucklefish.stardewvalley/files/Mods/VirtualKeyboard"
 adb push "bin/Release/net5.0/manifest.json" "/storage/emulated/0/Android/data/com.chucklefish.stardewvalley/files/Mods/VirtualKeyboard"

adb shell am start -n com.chucklefish.stardewvalley/crc64e8b22d3833b21ea5.MainActivity
