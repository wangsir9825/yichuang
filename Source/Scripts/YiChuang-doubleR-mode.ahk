; <COMPILER: v1.1.33.06>
#NoEnv
SendMode Input
SetWorkingDir %A_ScriptDir%
#NoTrayIcon
if (A_Args.Length() < 1)
    ExitApp
mainWindow := A_Args[1] + 0
NotifySelection() {
    global mainWindow
    target := WinExist("A")
    if (target && target != mainWindow)
        PostMessage, 0x83C0, 3, %target%,, ahk_id %mainWindow%
}

~LButton & RButton::
Send {LButton up}
NotifySelection()
return
RButton::
RButtonCount++
if RButtonCount=1
SetTimer,CheckR,-1000
if RButtonCount=1
KeyWait,RButton,U,T0.8
if ErrorLevel
RButtonCount1:=1
else
{
KeyWait,RButton,D,T0.1
if ErrorLevel
RButtonCount1:=2
else
{
RButtonCount++
RButtonCount1:=3
}
}
if RButtonCount1=1
Menu, tray, Show
else if RButtonCount1=2
Send {RButton}
else if RButtonCount1=3
NotifySelection()
if  (RButtonCount1=1 || RButtonCount1=3)
gosub,RCount
Return
RCount:
RButtonCount:=0
RButtonCount1:=0
return
CheckR:
if (RButtonCount=1 && RButtonCount1=2)
gosub,RCount
return
