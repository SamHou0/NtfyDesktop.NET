#!/bin/bash

export NO_STRIP=true
export APPIMAGETOOL=$HOME/tools/appimagetool-x86_64.AppImage

rm -r AppDir

mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps

dotnet publish -o AppDir/usr/bin -c Release NtfyDesktop.NET/NtfyDesktop.NET.csproj

cp NtfyDesktop.NET.desktop AppDir/usr/share/applications/

cp NtfyDesktop.NET/Assets/Ntfy.png AppDir/usr/share/icons/hicolor/256x256/apps/NtfyDesktop.NET.png

~/tools/linuxdeploy-x86_64.AppImage --appdir AppDir --output appimage --desktop-file AppDir/usr/share/applications/NtfyDesktop.NET.desktop --icon-file AppDir/usr/share/icons/hicolor/256x256/apps/NtfyDesktop.NET.png