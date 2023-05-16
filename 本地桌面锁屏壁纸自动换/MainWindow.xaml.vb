Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports Windows.Storage
Imports Microsoft.Win32
Imports System.ComponentModel

Class MainWindow
	Private Sub 更新当前桌面() Handles 桌面壁纸列表.MouseLeftButtonUp
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		Dim 有效监视器 As New List(Of 监视器设备)
		For a As Byte = 0 To 监视器个数
			Dim 新设备 As New 监视器设备(a)
			If 新设备.有效 Then
				有效监视器.Add(新设备)
			End If
		Next
		Try
			桌面壁纸列表.ItemsSource = 有效监视器
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Sub 更新当前锁屏() Handles 锁屏_当前图片.MouseLeftButtonUp
		Static 用户SID As String = Security.Principal.WindowsIdentity.GetCurrent.User.Value
		Static 锁屏搜索目录 As String = IO.Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", 用户SID, "ReadOnly")
		If Not IO.Directory.Exists(锁屏搜索目录) Then
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If
		Static 锁屏注册表路径 As String = IO.Path.Combine("SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData", 用户SID, "AnyoneRead\LockScreen")
		Dim 锁屏注册表 As RegistryKey = Registry.LocalMachine.OpenSubKey(锁屏注册表路径)
		If 锁屏注册表 Is Nothing Then
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If
		锁屏_当前图片.Source = New BitmapImage(New Uri(IO.Path.Combine(锁屏搜索目录, "LockScreen_" & DirectCast(锁屏注册表.GetValue(Nothing), String).Chars(0), "LockScreen.jpg")))
	End Sub

	ReadOnly 目录浏览对话框 As New Pickers.FolderPicker

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		更新当前桌面()
		更新当前锁屏()
		桌面_更换周期.SelectedIndex = Settings.桌面轮换周期
		锁屏_更换周期.SelectedIndex = Settings.锁屏轮换周期
		桌面_图集目录.Text = Settings.所有桌面目录
		锁屏_图集目录.Text = Settings.所有锁屏目录
		Current.当前窗口 = Me
		AddHandler 桌面_更换周期.SelectionChanged, AddressOf 桌面_更换周期_SelectionChanged
		AddHandler 锁屏_更换周期.SelectionChanged, AddressOf 锁屏_更换周期_SelectionChanged
		AddHandler 自动换_桌面, AddressOf 更新当前桌面
		AddHandler 自动换_锁屏, AddressOf 更新当前锁屏
	End Sub

	Private Async Sub 桌面_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			Settings.所有桌面目录 = 目录.Path
			桌面_图集目录.Text = Settings.所有桌面目录
		End If
	End Sub

	Private Async Sub 锁屏_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			Settings.所有锁屏目录 = 目录.Path
			锁屏_图集目录.Text = Settings.所有锁屏目录
		End If
	End Sub

	Private Sub 桌面_立即更换_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_立即更换.Click
		Try
			换桌面()
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Async Sub 锁屏_立即更换_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_立即更换.Click
		Try
			Await 换锁屏()
		Catch ex As Exception
			锁屏图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Sub 桌面_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		Settings.桌面轮换周期 = 桌面_更换周期.SelectedIndex
		更换周期("桌面", (Settings.桌面轮换周期, Settings.锁屏轮换周期), 桌面定时器, Settings.上次桌面时间)
		Current.消息($"桌面周期设置 {桌面_更换周期.SelectedValue}")
	End Sub

	Private Sub 锁屏_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		Settings.锁屏轮换周期 = 锁屏_更换周期.SelectedIndex
		更换周期("锁屏", (Settings.锁屏轮换周期, Settings.桌面轮换周期), 锁屏定时器, Settings.上次锁屏时间)
		Current.消息($"锁屏周期设置 {锁屏_更换周期.SelectedValue}")
	End Sub

	Private Sub 日志_Click(sender As Object, e As RoutedEventArgs) Handles 日志.Click
		Process.Start("notepad.exe", Current.日志路径)
	End Sub

	Private Sub 反馈_Click(sender As Object, e As RoutedEventArgs) Handles 反馈.Click
		Process.Start(New ProcessStartInfo("mailto:Leenrung@outlook.com?subject=本地桌面锁屏壁纸自动换 应用反馈&body=请将日志附在邮件中") With {.UseShellExecute = True})
	End Sub

	Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
		'只能在Loaded中初始化，因为构造阶段窗口还没有句柄
		WinRT.Interop.InitializeWithWindow.Initialize(目录浏览对话框, New Interop.WindowInteropHelper(Me).Handle)
		目录浏览对话框.FileTypeFilter.Add("*")
	End Sub

	Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
		Settings.Save()
	End Sub
End Class
