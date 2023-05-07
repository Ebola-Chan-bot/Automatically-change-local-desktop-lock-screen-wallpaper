Imports Microsoft.Win32
Imports 本地桌面锁屏壁纸自动换.My
Imports System.IO

Class MainWindow
	'TileWallpaper*23+WallPaper
	Enum 桌面壁纸样式 As Byte
		填充 = 10
		适应 = 6
		拉伸 = 2
		平铺 = 23
		居中 = 0
		跨区 = 22
	End Enum

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		If Settings.当前桌面目录 = "" Then
			Settings.当前桌面目录 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes")
		End If
		If Settings.当前锁屏目录 = "" Then
			Settings.当前锁屏目录 = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", Security.Principal.WindowsIdentity.GetCurrent.User.Value, "ReadOnly")
		End If
		桌面_当前搜索路径.Text = Settings.当前桌面目录
		锁屏_当前搜索路径.Text = Settings.当前锁屏目录
		桌面_更换周期.SelectedIndex = Settings.桌面轮换周期
		锁屏_更换周期.SelectedIndex = Settings.锁屏轮换周期
		桌面_浏览图集.Content = Settings.所有桌面目录
		锁屏_浏览图集.Content = Settings.所有锁屏目录
		Dim 当前桌面壁纸路径 As String() = Nothing
		If Forms.Screen.AllScreens.Length > 1 Then
			Static Desktop As RegistryKey = Registry.CurrentUser.OpenSubKey("Control Panel\Desktop")
			Dim TileWallpaper As String = Desktop.GetValue("TileWallpaper")
			Dim WallpaperStyle As String = Desktop.GetValue("WallpaperStyle")
			Select Case TileWallpaper * 23 + WallpaperStyle
				Case 桌面壁纸样式.填充, 桌面壁纸样式.适应, 桌面壁纸样式.拉伸
					当前桌面壁纸路径 = Directory.GetFiles(Settings.当前桌面目录, "Transcoded_*")
				Case 桌面壁纸样式.平铺, 桌面壁纸样式.居中, 桌面壁纸样式.跨区
					当前桌面壁纸路径 = Directory.GetFiles(Settings.当前桌面目录, "TranscodedWallpaper")
				Case Else
					桌面图片错误.Text = $"桌面壁纸样式解析失败：TileWallpaper={TileWallpaper}, WallpaperStyle={WallpaperStyle}"
			End Select
		Else
			当前桌面壁纸路径 = Directory.GetFiles(Path.Combine(Settings.当前桌面目录, "CachedFiles"), "CachedImage_*.jpg")
		End If
		If 当前桌面壁纸路径 IsNot Nothing Then
			桌面壁纸列表.ItemsSource = 当前桌面壁纸路径
		End If
		Dim 锁屏文件 As String = Nothing
		Dim 最新修改 As Date = Date.MinValue
		For Each 目录 As String In Directory.GetDirectories(Settings.当前锁屏目录)
			Dim 文件 As String = Path.Combine(目录, "LockScreen.jpg")
			If File.Exists(文件) Then
				Dim 修改 As Date = File.GetLastWriteTime(文件)
				If 修改 > 最新修改 Then
					锁屏文件 = 文件
					最新修改 = 修改
				End If
			End If
		Next
		If 锁屏文件 Is Nothing Then
			锁屏图片错误.Text = "没有找到锁屏图片"
		Else
			锁屏_当前图片.Source = New BitmapImage(New Uri(锁屏文件))
		End If
	End Sub
End Class
