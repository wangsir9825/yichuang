; Main app owns the configurable shortcut. This helper only copies once.
#NoEnv
#NoTrayIcon
#SingleInstance Off
SendMode Input
SetWorkingDir %A_ScriptDir%
if (A_Args.Length() < 2 || A_Args[1] != "capture")
    ExitApp
captureWindow := A_Args[2] + 0
KeyWait, Control, T2
if ErrorLevel
    ExitApp
KeyWait, Alt, T2
if ErrorLevel
    ExitApp
KeyWait, Shift, T2
if ErrorLevel
    ExitApp
if (WinActive("ahk_id " captureWindow))
    SendInput, ^c
ExitApp
