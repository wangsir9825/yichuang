param([string]$AppDirectory=(Join-Path $PSScriptRoot 'App'))
$ErrorActionPreference='Stop'
# Update only the script resource in the existing AutoHotkey runtimes; no download or global hotkeys run during build.
if (-not ('YiChuangBuildResources' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
public static class YiChuangBuildResources {
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr BeginUpdateResource(string file,bool delete);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool UpdateResource(IntPtr handle,IntPtr type,string name,ushort language,byte[] data,uint length);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern bool EndUpdateResource(IntPtr handle,bool discard);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr LoadLibraryEx(string file,IntPtr reserved,uint flags);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern IntPtr FindResourceEx(IntPtr module,IntPtr type,string name,ushort language);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern IntPtr LoadResource(IntPtr module,IntPtr resource);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern uint SizeofResource(IntPtr module,IntPtr resource);
    [DllImport("kernel32.dll",SetLastError=true)] public static extern IntPtr LockResource(IntPtr resource);
    [DllImport("kernel32.dll")] public static extern bool FreeLibrary(IntPtr module);
    public static byte[] Read(string file) {
        IntPtr module=LoadLibraryEx(file,IntPtr.Zero,0x22);if(module==IntPtr.Zero)throw new Win32Exception();
        try {IntPtr resource=FindResourceEx(module,new IntPtr(10),">AUTOHOTKEY SCRIPT<",1033);if(resource==IntPtr.Zero)throw new Win32Exception();
            byte[] bytes=new byte[SizeofResource(module,resource)];Marshal.Copy(LockResource(LoadResource(module,resource)),bytes,0,bytes.Length);return bytes;
        } finally {FreeLibrary(module);}
    }
}
'@
}
$appRoot=[IO.Path]::GetFullPath($AppDirectory)
foreach($mode in @('select','F1','doubleR')) {
    $exe=[IO.Path]::GetFullPath((Join-Path $appRoot ('XtransNoLogin-'+$mode+'-mode.exe')))
    if(-not $exe.StartsWith($appRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Helper path escaped App'}
    if(-not (Test-Path -LiteralPath $exe)){throw ('Existing helper runtime missing: '+$exe)}
    $source=Join-Path $PSScriptRoot ('Source/Scripts/Xtranslator-'+$mode+'-mode.ahk')
    $text=[IO.File]::ReadAllText($source)
    if($text -match '(?m)^![1-6]::|WinMove|SysGet'){throw 'Removed window-position hotkeys remain in source'}
    $bytes=[Text.Encoding]::UTF8.GetBytes([char]0xfeff+$text)
    $handle=[YiChuangBuildResources]::BeginUpdateResource($exe,$false)
    if($handle -eq [IntPtr]::Zero){throw 'Cannot open helper script resource for update'}
    try {
        if(-not [YiChuangBuildResources]::UpdateResource($handle,[IntPtr]10,'>AUTOHOTKEY SCRIPT<',1033,$bytes,$bytes.Length)){throw 'Helper script update failed'}
        if(-not [YiChuangBuildResources]::EndUpdateResource($handle,$false)){throw 'Cannot commit helper script resource'}
        $handle=[IntPtr]::Zero
    } finally {if($handle -ne [IntPtr]::Zero){[YiChuangBuildResources]::EndUpdateResource($handle,$true) | Out-Null}}
    $actual=[YiChuangBuildResources]::Read($exe)
    if([Convert]::ToBase64String($actual) -ne [Convert]::ToBase64String($bytes)){throw 'Compiled helper script differs from source'}
    Write-Output ($mode+' helper script verified; removed Alt+1-6/window-moving handlers absent.')
}
