set MSB=%WinDir%\Microsoft.NET\Framework\v4.0.30319\MSBuild

erase "Releases\*.*" /Q

echo Compiling V1
xcopy "Incom.Yasen.Content\Yasen.UI\Properties\AssemblyInfoV1.cs" "Incom.Yasen.Content\Yasen.UI\Properties\AssemblyInfo.cs" /y
%MSB% "Incom.Yasen.Deploy\Yasen.Setup.Kaliningrad\Yasen.Setup.Kaliningrad.Wix.sln" /t:Rebuild /p:Configuration=Release
xcopy "Incom.Yasen.Deploy\Yasen.Setup.Kaliningrad\bin\Release\YasenSetup.msi" "Releases\" /Y
rename "Releases\YasenSetup.msi" YasenSetup1.msi

echo Compiling V1.0.1
xcopy "Incom.Yasen.Content\Yasen.UI\Properties\AssemblyInfoV1.0.1.cs" "Incom.Yasen.Content\Yasen.UI\Properties\AssemblyInfo.cs" /y
%MSB% "Incom.Yasen.Deploy\Yasen.Setup.Kaliningrad\Yasen.Setup.Kaliningrad.Wix.sln" /t:Rebuild /p:Configuration=Release
xcopy "Incom.Yasen.Deploy\Yasen.Setup.Kaliningrad\bin\Release\YasenSetup.msi" "Releases\" /Y
rename "Releases\YasenSetup.msi" YasenSetup1.0.1.msi

echo compile patching tool
%MSB% "DeployUtils\Incom.MakeMsp\MakeMsp.sln" /t:Build /p:Configuration=Release

echo creating patch
DeployUtils\Incom.MakeMsp\bin\Release\MakeMsp.exe "Releases\YasenSetup1.msi" "Releases\YasenSetup1.0.1.msi" "Incom.Yasen.Deploy\Deploy.Yasen.PatchCreation.xml" "Releases\Patch.msp"

echo done!