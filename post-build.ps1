$Precompile = "$PSScriptRoot/Lib/protobuf-net-precompile/precompile.exe"
$UnityAssemblies = "$PSScriptRoot/EcsSync2FpsUnity/Assets/Scripts/Assemblies"
$SourceAssemblies = "$PSScriptRoot/EcsSync2Fps/bin/Debug/net35"

cp -Force $SourceAssemblies/EcsSync2.* $UnityAssemblies
cp -Force $SourceAssemblies/EcsSync2Fps.* $UnityAssemblies

# & $Precompile -o:"$UnityAssemblies/EcsSync2FpsSerializers.dll" -t:"EcsSync2.Fps.Serializers" "$SourceAssemblies/EcsSync2.dll" "$SourceAssemblies/EcsSync2Fps.dll"