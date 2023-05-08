Imports System.IO
Imports Microsoft.Win32
''' <summary>
''' 不同的显示器有可能会共享相同的壁纸
''' </summary>
Public Structure 屏幕壁纸
	Property 设备名称 As String
	Property 壁纸路径 As String
End Structure

Public Enum 桌面壁纸样式 As Byte
	填充 = 10
	适应 = 6
	拉伸 = 2
	平铺 = 23
	居中 = 0
	跨区 = 22
End Enum

Public Module 桌面
	''' <summary>
	''' 获取当前桌面壁纸样式
	''' </summary>
	Function 壁纸样式() As 桌面壁纸样式
		Static Desktop As RegistryKey = Registry.CurrentUser.OpenSubKey("Control Panel\Desktop")
		Return Desktop.GetValue("TileWallpaper") * 23 + Desktop.GetValue("WallpaperStyle")
	End Function
	''' <summary>
	''' 从指定路径搜索各显示器的壁纸
	''' </summary>
	''' <returns>屏幕壁纸数组，包含每个显示器与壁纸路径的对应关系</returns>
	Function 各屏壁纸(搜索路径 As String) As 屏幕壁纸()
		Dim 所有屏幕 As Screen() = Screen.AllScreens()
		Dim 当前桌面壁纸路径 As String()
		If Screen.AllScreens.Length > 1 Then
			Select Case 壁纸样式()
				Case 桌面壁纸样式.填充, 桌面壁纸样式.适应, 桌面壁纸样式.拉伸
					当前桌面壁纸路径 = Directory.GetFiles(搜索路径, "Transcoded_*")
				Case 桌面壁纸样式.平铺, 桌面壁纸样式.居中, 桌面壁纸样式.跨区
					当前桌面壁纸路径 = Directory.GetFiles(搜索路径, "TranscodedWallpaper")
				Case Else
					Throw New Exception("桌面壁纸样式解析失败")
			End Select
		Else
			当前桌面壁纸路径 = Directory.GetFiles(Path.Combine(搜索路径, "CachedFiles"), "CachedImage_*.jpg")
		End If
		Dim 返回(所有屏幕.Length - 1) As 屏幕壁纸
		For a As Byte = 0 To 所有屏幕.Length - 1
			返回(a).设备名称 = 所有屏幕(a).DeviceName
			返回(a).壁纸路径 = 当前桌面壁纸路径(a Mod 当前桌面壁纸路径.Length)
		Next
		Return 返回
	End Function

	Public ReadOnly 默认搜索路径 As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes")
	''' <summary>
	''' 从默认搜索路径查找当前桌面壁纸
	''' </summary>
	''' <returns>屏幕壁纸数组，包含每个显示器与壁纸路径的对应关系</returns>
	Function 各屏壁纸() As 屏幕壁纸()
		Return 各屏壁纸(默认搜索路径)
	End Function
End Module
