Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports Windows.Storage
Imports Microsoft.Win32
Imports System.Security.Principal

Class MainWindow
	Private Structure 桌面呈现结构
		ReadOnly Property 监视器 As String
		Property 壁纸图 As BitmapImage
		Property 更换周期 As 轮换周期
			Get
				Return If(注册表更换周期.GetValue(监视器), 轮换周期.默认)
			End Get
			Set(value As 轮换周期)
				注册表更换周期.SetValue(监视器, value)
			End Set
		End Property
		Property 图集目录 As String
			Get
				Return If(注册表图集目录.GetValue(监视器), "默认")
			End Get
			Set(value As String)
				注册表图集目录.SetValue(监视器, value)
			End Set
		End Property
		Sub New(监视器 As String)
			Me.监视器 = 监视器
		End Sub
	End Structure

	Private Sub 更新当前桌面() Handles 桌面壁纸列表.MouseLeftButtonUp
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		Dim 有效监视器 As New List(Of 桌面呈现结构)
		Dim 缓存壁纸 As String()
		For a As Byte = 0 To 监视器个数
			Dim 新设备 As New 监视器设备(a)
			If 新设备.有效 Then
				Dim 呈现 As New 桌面呈现结构(新设备.路径名称)
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
		Static 用户SID As String = WindowsIdentity.GetCurrent.User.Value
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
		Dim 锁屏历史路径 As String = 锁屏注册表.GetValue(Nothing)
		If 锁屏历史路径 Is Nothing Then
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If

		锁屏历史路径 = IO.Path.Combine(锁屏搜索目录, "LockScreen_" & 锁屏历史路径.Chars(0), "LockScreen.jpg")
		If Not IO.File.Exists(锁屏历史路径) Then
			'没有任何锁屏历史时，此路径可能是无效的
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If

		'锁屏图是经过转码的，即使图片有颜色上下文的损坏也会被修复
		锁屏_当前图片.Source = New BitmapImage(New Uri(锁屏历史路径))
	End Sub

	ReadOnly 目录浏览对话框 As New Pickers.FolderPicker

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		更新当前桌面()
		更新当前锁屏()
		桌面_更换周期.SelectedIndex = If(注册表更换周期.GetValue("桌面"), 轮换周期.禁用)
		锁屏_更换周期.SelectedIndex = If(注册表更换周期.GetValue("锁屏"), 轮换周期.禁用)
		桌面_图集目录.Text = If(注册表图集目录.GetValue("桌面"), "")
		锁屏_图集目录.Text = If(注册表图集目录.GetValue("锁屏"), "")
		Current.当前窗口 = Me
		AddHandler 桌面_更换周期.SelectionChanged, AddressOf 桌面_更换周期_SelectionChanged
		AddHandler 锁屏_更换周期.SelectionChanged, AddressOf 锁屏_更换周期_SelectionChanged
		AddHandler 自动换_桌面, AddressOf 更新当前桌面
		AddHandler 自动换_锁屏, AddressOf 更新当前锁屏
	End Sub

	Private Async Sub 桌面_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			注册表图集目录.SetValue("桌面", 目录.Path)
			桌面_图集目录.Text = 目录.Path
		End If
	End Sub

	Private Async Sub 锁屏_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			注册表图集目录.SetValue("锁屏", 目录.Path)
			锁屏_图集目录.Text = 目录.Path
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
		注册表更换周期.SetValue("桌面", 桌面_更换周期.SelectedIndex)
		更换周期("桌面", (桌面_更换周期.SelectedIndex, 锁屏_更换周期.SelectedIndex), 桌面定时器, 注册表根.GetValue("上次桌面时间"))
		Current.消息($"桌面周期设置 {桌面_更换周期.SelectedValue}")
	End Sub

	Private Sub 锁屏_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		注册表更换周期.SetValue("锁屏", 锁屏_更换周期.SelectedIndex)
		更换周期("锁屏", (锁屏_更换周期.SelectedIndex, 桌面_更换周期.SelectedIndex), 锁屏定时器, Settings.上次锁屏时间)
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
		winrt.Interop.InitializeWithWindow.Initialize(目录浏览对话框, New Interop.WindowInteropHelper(Me).Handle)
		目录浏览对话框.FileTypeFilter.Add("*")
	End Sub

	Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing
		Settings.Save()
	End Sub
End Class
