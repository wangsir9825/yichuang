; <COMPILER: v1.1.33.02>
#NoEnv
SendMode Input
SetWorkingDir %A_ScriptDir%
~$Lbutton::
minGap  = 30
mousegetpos xpos1,ypos1
Keywait, Lbutton, U; 等待鼠标左键弹起
mousegetpos xpos2, ypos2
if ( abs(xpos2-xpos1) > minGap or abs(ypos1-ypos2) > minGap)
send, ^c
return
