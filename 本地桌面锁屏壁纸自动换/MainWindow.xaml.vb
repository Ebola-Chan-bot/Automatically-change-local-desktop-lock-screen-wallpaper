Imports System.ComponentModel
Imports System.IO
Imports System.Security.Principal
Imports System.Threading
Imports Microsoft.Win32
Imports Windows.Storage
Imports 桌面壁纸取设

Class MainWindow
	Protected 桌面未加载 As Boolean = True
	Protected 锁屏未加载 As Boolean = True
	Shared Function 载入位图(路径 As String) As BitmapImage
		Dim 壁纸路径 As New Uri(路径)
		Try
			载入位图 = New BitmapImage(壁纸路径)
		Catch ex As IOException
			载入位图 = New BitmapImage
			With 载入位图
				.BeginInit()
				.CreateOptions = BitmapCreateOptions.IgnoreColorProfile
				.UriSource = 壁纸路径
				.EndInit()
			End With
		End Try
	End Function
	Private Class 立即更换
		Implements ICommand
		Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
		ReadOnly 父 As 桌面呈现结构
		Sub New(父 As 桌面呈现结构)
			Me.父 = 父
		End Sub
		Public Sub Execute(parameter As Object) Implements ICommand.Execute
			Try
				Dim 所有图片 As String()
				Dim 图集目录 As String = 父.注册表键.GetValue("图集目录")
				Dim 监视器ID = 父.监视器ID
				If IsNothing(图集目录) Then
					Dim 默认图集目录 As String = 默认桌面.GetValue("图集目录")
					Try
						所有图片 = Directory.GetFiles(默认图集目录)
					Catch ex As ArgumentException
						Throw New 监视器异常("图集目录无效", 监视器ID, 默认图集目录, ex)
					End Try
				Else
					Try
						所有图片 = Directory.GetFiles(图集目录)
					Catch ex As ArgumentException
						Throw New 监视器异常("图集目录无效", 监视器ID, 图集目录, ex)
					End Try
				End If
				If 所有图片.Length = 0 Then
					Throw New 监视器异常("图集目录没有图片", 监视器ID, 图集目录)
				End If
				Dim 监视器 As New 监视器设备(父.监视器ID)
				If 监视器.有效 Then
					Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
					监视器.壁纸路径 = 壁纸路径
					消息($"{监视器ID} 设置桌面 {壁纸路径}")
					父.注册表键.SetValue("上次时间", Now)
					父.壁纸图 = 载入位图(壁纸路径)
					父.文件名 = Path.GetFileName(壁纸路径)
					父.错误消息 = ""
				Else
					父.注册表键.SetValue("有效", False)
					DirectCast(父.主窗口.桌面壁纸列表.ItemsSource, List(Of 桌面呈现结构)).Remove(父)
				End If
			Catch ex As Exception
				父.错误消息 = 报错(ex)
			End Try
		End Sub
		Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
			Return True
		End Function
	End Class
	Private Class 浏览
		Implements ICommand
		ReadOnly 父 As 桌面呈现结构
		Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged
		Sub New(父 As 桌面呈现结构)
			Me.父 = 父
		End Sub
		Public Async Sub Execute(parameter As Object) Implements ICommand.Execute
			父.图集目录 = (Await 父.主窗口.目录浏览对话框.PickSingleFolderAsync)?.Path
		End Sub

		Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
			Return True
		End Function
	End Class
	Private Class 桌面呈现结构
		Implements INotifyPropertyChanged
		Friend ReadOnly 注册表键 As RegistryKey
		Friend ReadOnly 主窗口 As MainWindow
		Sub New(监视器 As String, 主窗口 As MainWindow)
			注册表键 = 注册表根.CreateSubKey(Convert.ToBase64String(Text.Encoding.Unicode.GetBytes(监视器)).Replace("/"c, "-"c))
			Me.主窗口 = 主窗口
			注册表键.SetValue("有效", True)
			监视器ID = 监视器
		End Sub
		ReadOnly Property 监视器ID As String
		Property 文件名 As String
			Get
				Return 注册表键.GetValue("文件名")
			End Get
			Set(value As String)
				注册表键.SetValue("文件名", value)
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(文件名)))
			End Set
		End Property
		Private i壁纸图 As BitmapImage
		Property 壁纸图 As BitmapImage
			Get
				Return i壁纸图
			End Get
			Set(value As BitmapImage)
				i壁纸图 = value
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(壁纸图)))
			End Set
		End Property
		Property 更换周期 As Byte
			Get
				Return 注册表键.GetValue("更换周期", 轮换周期.默认)
			End Get
			Set(value As Byte)
				注册表键.SetValue("更换周期", value)
				消息($"{监视器ID} 更换周期设置 {DirectCast(value, 轮换周期)}")

				'此更改可能导致下次唤醒时间变得更接近，所以需要全部重新计算
				检查更换设置唤醒()
			End Set
		End Property
		Property 图集目录 As String
			Get
				Return 注册表键.GetValue("图集目录", Nothing)
			End Get
			Set(value As String)
				If value Is Nothing Then
					注册表键.DeleteValue("图集目录", False)
				Else
					注册表键.SetValue("图集目录", value)
					RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(图集目录)))
				End If
			End Set
		End Property
		Protected i错误消息 As String
		Property 错误消息 As String
			Get
				Return i错误消息
			End Get
			Set(value As String)
				i错误消息 = value
				RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(错误消息)))
			End Set
		End Property
		Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
		ReadOnly Property 立即更换命令 As New 立即更换(Me)
		ReadOnly Property 浏览命令 As New 浏览(Me)
		Sub New(新设备 As 监视器设备, ByRef 缓存壁纸 As String(), 主窗口 As MainWindow, 监视器索引 As Byte)
			Me.主窗口 = 主窗口
			Dim 路径字符串 As String = 新设备.壁纸路径
			监视器ID = 新设备.路径名称
			注册表键 = 注册表根.CreateSubKey(Convert.ToBase64String(Text.Encoding.Unicode.GetBytes(监视器ID)).Replace("/"c, "-"c))
			文件名 = Path.GetFileName(路径字符串)
			If Not File.Exists(路径字符串) Then
				If 缓存壁纸 Is Nothing Then
					Dim Themes As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes")
					Dim CachedFiles As String = IO.Path.Combine(Themes, "CachedFiles")
					If Directory.Exists(CachedFiles) Then
						缓存壁纸 = Directory.GetFiles(CachedFiles)
					Else
						Select Case 桌面壁纸.位置
							Case 桌面壁纸位置.填充, 桌面壁纸位置.适应, 桌面壁纸位置.拉伸
								缓存壁纸 = Directory.GetFiles(Themes, "Transcoded_*")
							Case 桌面壁纸位置.平铺, 桌面壁纸位置.居中, 桌面壁纸位置.跨区
								缓存壁纸 = Directory.GetFiles(Themes, "TranscodedWallpaper")
						End Select
					End If
				End If
				路径字符串 = 缓存壁纸(If(缓存壁纸.Length > 1, 监视器索引, 0))
			End If
			壁纸图 = 载入位图(路径字符串)

			'必须放在最后设置，因为前面有可能出错退出
			注册表键.SetValue("有效", True)
		End Sub
	End Class
	Private Sub 更新当前桌面() Handles 桌面壁纸列表.MouseLeftButtonUp
		桌面未加载 = False
		Try
			Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
			Dim 有效监视器 As New List(Of 桌面呈现结构)
			Dim 缓存壁纸 As String() = Nothing
			For a As Byte = 0 To 监视器个数
				Dim 新设备 As New 监视器设备(a)
				If 新设备.有效 Then
					Try
						有效监视器.Add(New 桌面呈现结构(新设备, 缓存壁纸, Me, a))
					Catch ex As Exception
						Continue For
					End Try
				End If
			Next
			桌面壁纸列表.ItemsSource = 有效监视器
		Catch ex As Exception
			桌面图片错误.Text = 报错(ex)
		End Try
	End Sub
	Private Sub 更新当前锁屏() Handles 锁屏_当前图片.MouseLeftButtonUp
		锁屏未加载 = False
		Static 用户SID As String = WindowsIdentity.GetCurrent.User.Value
		Static 锁屏搜索目录 As String = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", 用户SID, "ReadOnly")
		If Not Directory.Exists(锁屏搜索目录) Then
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If
		Static 锁屏注册表路径 As String = Path.Combine("SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData", 用户SID, "AnyoneRead\LockScreen")
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

		锁屏历史路径 = Path.Combine(锁屏搜索目录, "LockScreen_" & 锁屏历史路径.Chars(0), "LockScreen.jpg")
		If Not File.Exists(锁屏历史路径) Then
			'没有任何锁屏历史时，此路径可能是无效的
			锁屏图片错误.Text = "用户当前未设置任何个性化锁屏"
			Exit Sub
		End If

		'锁屏图是经过转码的，即使图片有颜色上下文的损坏也会被修复
		锁屏_当前图片.Source = New BitmapImage(New Uri(锁屏历史路径))
		锁屏文件名.Text = 默认锁屏.GetValue("文件名")
	End Sub
	Private Sub 自动换锁屏事件(异常消息 As String)
		锁屏图片错误.Text = 异常消息
		If 异常消息 Is Nothing Then
			更新当前锁屏()
		End If
	End Sub
	ReadOnly 目录浏览对话框 As New Pickers.FolderPicker
	Private Async Sub 桌面_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			默认桌面.SetValue("图集目录", 目录.Path)
			桌面_图集目录.Text = 目录.Path
		End If
	End Sub
	Private Async Sub 锁屏_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_浏览图集.Click
		Dim 目录 As StorageFolder = Await 目录浏览对话框.PickSingleFolderAsync
		If 目录 IsNot Nothing Then
			默认锁屏.SetValue("图集目录", 目录.Path)
			锁屏_图集目录.Text = 目录.Path
		End If
	End Sub
	Private Sub 桌面_立即更换_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_立即更换.Click
		Try
			Dim 所有桌面 As New List(Of 桌面呈现结构)
			Dim 默认图集 As String() = Nothing
			Dim 缓存壁纸 As String() = Nothing
			Dim 无默认图集 As Boolean = False
			Dim 默认图集目录 As String = Nothing
			For M As Byte = 0 To 监视器设备.监视器设备计数() - 1
				Try
					Dim 监视器 As New 监视器设备(M)
					If 监视器.有效 Then
						Dim 监视器ID As String = 监视器.路径名称
						Dim 桌面 As New 桌面呈现结构(监视器ID, Me)
						Try
							Dim 图集目录 As String = 桌面.注册表键.GetValue("图集目录")
							Dim 所有图片 As String()
							If 图集目录 Is Nothing Then
								If 无默认图集 Then
									Throw New 监视器异常("图集目录无效", 监视器ID, 默认图集目录)
								End If
								If 默认图集 Is Nothing Then
									默认图集目录 = 默认桌面.GetValue("图集目录")
									Try
										默认图集 = Directory.GetFiles(默认图集目录)
									Catch ex As ArgumentException
										无默认图集 = True
										Throw New 监视器异常("图集目录无效", 监视器ID, 默认图集目录, ex)
									End Try
								End If
								所有图片 = 默认图集
								If 所有图片 Is Nothing Then
									无默认图集 = True
									Throw New 监视器异常("图集目录无效", 监视器ID, 默认图集目录)
								End If
							Else
								Try
									所有图片 = Directory.GetFiles(图集目录)
								Catch ex As ArgumentException
									Throw New 监视器异常("图集目录无效", 监视器ID, 图集目录, ex)
								End Try
							End If
							If 所有图片.Length = 0 Then
								Throw New 监视器异常("图集目录没有图片", 监视器ID, 图集目录)
							End If
							Dim 选定图片 As String = 所有图片(随机生成器.Next(所有图片.Length))
							监视器.壁纸路径 = 选定图片
							桌面.壁纸图 = 载入位图(选定图片)
							桌面.文件名 = Path.GetFileName(选定图片)
							桌面.注册表键.SetValue("上次时间", Now)
							消息($"{监视器ID} 设置桌面 {选定图片}")
						Catch ex As Exception
							Try
								桌面 = New 桌面呈现结构(监视器, 缓存壁纸, Me, M) With {
								.错误消息 = 报错(ex)
							}
							Catch 无效监视器 As Exception
								Continue For
							End Try
						End Try
						所有桌面.Add(桌面)
					End If
				Catch ex As Exception
					桌面图片错误.Text = 报错(ex)
				End Try
			Next
			桌面壁纸列表.ItemsSource = 所有桌面
		Catch ex As Exception
			桌面图片错误.Text = 报错(ex)
		End Try
	End Sub

	Private Sub 桌面_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		默认桌面.SetValue("更换周期", 桌面_更换周期.SelectedIndex)
		消息($"桌面周期设置 {桌面_更换周期.SelectedValue}")
		检查更换设置唤醒()
	End Sub

	Private Sub 锁屏_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		默认锁屏.SetValue("更换周期", 锁屏_更换周期.SelectedIndex)
		消息($"锁屏周期设置 {锁屏_更换周期.SelectedValue}")
		检查更换设置唤醒()
	End Sub

	Private Sub 日志_Click(sender As Object, e As RoutedEventArgs) Handles 日志.Click
		Process.Start("notepad.exe", 日志路径)
	End Sub

	Private Sub 反馈_Click(sender As Object, e As RoutedEventArgs) Handles 反馈.Click
		Process.Start(New ProcessStartInfo("mailto:Leenrung@outlook.com?subject=本地桌面锁屏壁纸自动换 应用反馈&body=请将日志附在邮件中") With {.UseShellExecute = True})
	End Sub
	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。

		'全局事件的处理方法必须在窗口关闭后手动清理
		AddHandler 自动换_桌面, AddressOf 更新当前桌面
		AddHandler 自动换_锁屏, AddressOf 自动换锁屏事件

		检查更换设置唤醒()
	End Sub
	Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
		桌面_更换周期.SelectedIndex = 默认桌面.GetValue("更换周期", 轮换周期.禁用)
		锁屏_更换周期.SelectedIndex = 默认锁屏.GetValue("更换周期", 轮换周期.禁用)
		桌面_图集目录.Text = 默认桌面.GetValue("图集目录")
		锁屏_图集目录.Text = 默认锁屏.GetValue("图集目录")
		锁屏文件名.Text = 默认锁屏.GetValue("文件名")
		当前窗口 = Me
		AddHandler 桌面_更换周期.SelectionChanged, AddressOf 桌面_更换周期_SelectionChanged
		AddHandler 锁屏_更换周期.SelectionChanged, AddressOf 锁屏_更换周期_SelectionChanged
		AddHandler 锁屏_立即更换.Click, AddressOf 换锁屏

		'只能在Loaded中初始化，因为构造阶段窗口还没有句柄
		WinRT.Interop.InitializeWithWindow.Initialize(目录浏览对话框, New Interop.WindowInteropHelper(Me).Handle)
		目录浏览对话框.FileTypeFilter.Add("*")

		If 桌面未加载 Then
			更新当前桌面()
		End If
		If 锁屏未加载 Then
			更新当前锁屏()
		End If
	End Sub

	Private Sub MainWindow_Closed(sender As Object, e As EventArgs) Handles Me.Closed
		'在窗口关闭时，从全局事件中移除处理程序，以防止内存泄漏
		RemoveHandler 自动换_桌面, AddressOf 更新当前桌面
		RemoveHandler 自动换_锁屏, AddressOf 自动换锁屏事件
		'尽管其他控件的事件处理程序可能不会导致同样严重的内存泄漏（因为控件和窗口的生命周期相同），但在关闭时显式地移除所有用 AddHandler 添加的事件是一种良好的编程习惯。
		RemoveHandler 桌面_更换周期.SelectionChanged, AddressOf 桌面_更换周期_SelectionChanged
		RemoveHandler 锁屏_更换周期.SelectionChanged, AddressOf 锁屏_更换周期_SelectionChanged
		RemoveHandler 锁屏_立即更换.Click, DirectCast(AddressOf 换锁屏, RoutedEventHandler)

		当前窗口 = Nothing
		保留或关闭()
	End Sub
End Class
