$Precompile = "$PSScriptRoot/Lib/protobuf-net-precompile/precompile.exe"
$UnityAssemblies = "$PSScriptRoot/EcsSync2FpsUnity/Assets/Scripts/Assemblies"
$SourceAssemblies = "$PSScriptRoot/EcsSync2Fps/bin/Debug/net35"
$SystemCore = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll"

Copy-Item -Force $SourceAssemblies/EcsSync2.* $UnityAssemblies
Copy-Item -Force $SourceAssemblies/EcsSync2Fps.* $UnityAssemblies

& $Precompile -o:"$UnityAssemblies/EcsSync2FpsSerializers.dll" -t:"EcsSync2.Fps.Serializers" "$SourceAssemblies/EcsSync2.dll" "$SourceAssemblies/EcsSync2Fps.dll" $SystemCore