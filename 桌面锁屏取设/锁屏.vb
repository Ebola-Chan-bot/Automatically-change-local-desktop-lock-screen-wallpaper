Imports System.IO

Public Module 锁屏
	''' <summary>
	''' 从指定路径搜索当前锁屏壁纸
	''' </summary>
	''' <returns>锁屏壁纸路径</returns>
	Function 壁纸路径(搜索路径 As String) As String
		Dim 锁屏文件 As String = Nothing
		Dim 最新修改 As Date = Date.MinValue
		For Each 目录 As String In Directory.GetDirectories(搜索路径)
			Dim 文件 As String = Path.Combine(目录, "LockScreen.jpg")
			If File.Exists(文件) Then
				Dim 修改 As Date = File.GetLastWriteTime(文件)
				If 修改 > 最新修改 Then
					锁屏文件 = 文件
					最新修改 = 修改
				End If
			End If
		Next
		Return 锁屏文件
	End Function

	Public ReadOnly 默认搜索路径 As String = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", Security.Principal.WindowsIdentity.GetCurrent.User.Value, "ReadOnly")
	''' <summary>
	''' 从默认路径 %ProgramData%\Microsoft\Windows\SystemData\用户SID\ReadOnly 搜索当前锁屏壁纸
	''' </summary>
	''' <returns>锁屏壁纸路径</returns>
	Function 壁纸路径() As String
		Return 壁纸路径(默认搜索路径)
	End Function
End Module
