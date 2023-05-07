Imports System.IO
Imports Microsoft.Win32

Public Structure 屏幕壁纸
	Property 设备名称 As String
	Property 壁纸路径 As String
End Structure

Enum 桌面壁纸样式 As Byte
	填充 = 10
	适应 = 6
	拉伸 = 2
	平铺 = 23
	居中 = 0
	跨区 = 22
End Enum

Public Module 桌面
	Function 各屏壁纸(搜索路径 As String) As 屏幕壁纸()
		Dim 所有屏幕 As Screen() = Screen.AllScreens()
		所有屏幕(0).
		Dim 当前桌面壁纸路径 As String() = Nothing
		Try
			If Screen.AllScreens.Length > 1 Then
				Static Desktop As RegistryKey = Registry.CurrentUser.OpenSubKey("Control Panel\Desktop")
				Dim TileWallpaper As String = Desktop.GetValue("TileWallpaper")
				Dim WallpaperStyle As String = Desktop.GetValue("WallpaperStyle")
				Select Case TileWallpaper * 23 + WallpaperStyle
					Case 桌面壁纸样式.填充, 桌面壁纸样式.适应, 桌面壁纸样式.拉伸
						当前桌面壁纸路径 = Directory.GetFiles(搜索路径, "Transcoded_*")
					Case 桌面壁纸样式.平铺, 桌面壁纸样式.居中, 桌面壁纸样式.跨区
						当前桌面壁纸路径 = Directory.GetFiles(搜索路径, "TranscodedWallpaper")
					Case Else
						Throw New Exception($"桌面壁纸样式解析失败：TileWallpaper={TileWallpaper}, WallpaperStyle={WallpaperStyle}")
				End Select
			Else
				当前桌面壁纸路径 = Directory.GetFiles(Path.Combine(搜索路径, "CachedFiles"), "CachedImage_*.jpg")
			End If
			If 当前桌面壁纸路径 IsNot Nothing Then
			End If
		Catch ex As Exception
		End Try
	End Function

	Function 各屏壁纸() As 屏幕壁纸()
		Return 各屏壁纸(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes"))
	End Function
End Module
