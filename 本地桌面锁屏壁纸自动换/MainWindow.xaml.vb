Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports Windows.Storage
Imports Microsoft.Win32

Class MainWindow
	Private Structure 桌面呈现结构
		Property 监视器 As String
		Property 壁纸图 As BitmapImage
	End Structure

	Private Sub 更新当前桌面() Handles 桌面壁纸列表.MouseLeftButtonUp
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		Dim 有效监视器 As New List(Of 桌面呈现结构)
		Dim 缓存壁纸 As String()
		For a As Byte = 0 To 监视器个数
			Dim 新设备 As New 监视器设备(a)
			If 新设备.有效 Then
				Dim 呈现 As New 桌面呈现结构 With {.监视器 = 新设备.路径名称}
				Dim 路径字符串 As String = 新设备.壁纸路径
				If 路径字符串 = "" Then
					Continue For
				End If
				If Not IO.File.Exists(路径字符串) Then
					If 缓存壁纸 Is Nothing Then
						Dim Themes As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes")
						Dim CachedFiles As String = IO.Path.Combine(Themes, "CachedFiles")
						If IO.Directory.Exists(CachedFiles) Then
							缓存壁纸 = IO.Directory.GetFiles(CachedFiles)
						Else
							Select Case 桌面壁纸.位置
								Case 桌面壁纸位置.填充, 桌面壁纸位置.适应, 桌面壁纸位置.拉伸
									缓存壁纸 = IO.Directory.GetFiles(Themes, "Transcoded_*")
								Case 桌面壁纸位置.平铺, 桌面壁纸位置.居中, 桌面壁纸位置.跨区
									缓存壁纸 = IO.Directory.GetFiles(Themes, "TranscodedWallpaper")
								Case Else
									Continue For
							End Select
						End If
					End If
					路径字符串 = 缓存壁纸(If(缓存壁纸.Length > 1, a, 0))
				End If
				Dim 壁纸路径 As New Uri(路径字符串)
				Try
					呈现.壁纸图 = New BitmapImage(壁纸路径)
				Catch ex As IO.IOException
					呈现.壁纸图 = New BitmapImage
					With 呈现.壁纸图
						.BeginInit()
						.CreateOptions = BitmapCreateOptions.IgnoreColorProfile
						.UriSource = 壁纸路径
						.EndInit()
					End With
				End Try
				有效监视器.Add(呈现)
			End If
		Next
		Try
			桌面壁纸列表.ItemsSource = 有效监视器
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Sub 更新当前锁屏() Handles 锁屏_当前图片.MouseLeftButtonUp
		Dim 行号 As Byte = 0
		Try
			Static 用户SID As String = Security.Principal.WindowsIdentity.GetCurrent.User.Value
			行号 += 1 '1
			Static 锁屏搜索目录 As String = IO.Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", 用户SID, "ReadOnly")
			行号 += 1 '2
			If Not IO.Directory.Exists(锁屏搜索目录) Then
				行号 += 1 '3
				锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
				行号 += 1 '4
				Exit Sub
				行号 += 1 '5
			End If
			行号 += 1 '6
			Static 锁屏注册表路径 As String = IO.Path.Combine("SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData", 用户SID, "AnyoneRead\LockScreen")
			行号 += 1 '7
			Dim 锁屏注册表 As RegistryKey = Registry.LocalMachine.OpenSubKey(锁屏注册表路径)
			行号 += 1 '8
			If 锁屏注册表 Is Nothing Then
				行号 += 1 '9
				锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
				行号 += 1 '10
				Exit Sub
				行号 += 1 '11
			End If
			'锁屏图是经过转码的，即使图片有颜色上下文的损坏也会被修复
			行号 += 1 '12
			锁屏_当前图片.Source = New BitmapImage(New Uri(IO.Path.Combine(锁屏搜索目录, "LockScreen_" & DirectCast(锁屏注册表.GetValue(Nothing), String).Chars(0), "LockScreen.jpg")))
			行号 += 1 '13
		Catch ex As NullReferenceException
			Throw New NullReferenceException(ex.Message + $"行号：{行号}")
		End Try
	End Sub

	ReadOnly 目录浏览对话框 As New Pickers.FolderPicker

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
		WinRT.Interop.InitializeWithWindow.Initialize(目录浏览对话框, New Interop.WindowInteropHelper(Me).Handle)
		目录浏览对话框.FileTypeFilter.Add("*")
	End Sub

	Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
		Settings.Save()
	End Sub
End Class
