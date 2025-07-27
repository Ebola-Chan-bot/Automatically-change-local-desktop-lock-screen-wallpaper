Imports System.IO
Imports System.IO.Pipes
Imports System.Threading
Imports System.Windows.Threading
Imports Microsoft.Win32
Imports Windows.ApplicationModel
Imports Windows.Storage
Imports Windows.Storage.Streams
Imports 本地桌面锁屏壁纸自动换.My

Class Application

	' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
	' can be handled in this file.
	Friend WithEvents 当前窗口 As MainWindow
	Private 命名管道服务器流 As NamedPipeServerStream

	Private Sub 管道回调()
		Select Case 命名管道服务器流.ReadByte()
			Case 启动类型.用户启动
				If 当前窗口 Is Nothing Then
					Dispatcher.Invoke(Sub() Call (New MainWindow).Show())
				Else
					Dispatcher.Invoke(AddressOf 当前窗口.Activate)
				End If
			Case 启动类型.自启动
				自启动()
			Case 启动类型.换桌面
				Call 自动换桌面()
			Case 启动类型.换锁屏
				Call 自动换锁屏()
		End Select
		命名管道服务器流.Disconnect()
		命名管道服务器流.BeginWaitForConnection(AddressOf 管道回调, Nothing)
	End Sub

	Private 应该ShutDown As Boolean = False

	Sub New()
		Try
			命名管道服务器流 = New NamedPipeServerStream("本地桌面锁屏壁纸自动换", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous Or PipeOptions.CurrentUserOnly)
		Catch ex As IOException
			Dim 命名管道客户端流 As New NamedPipeClientStream(".", "本地桌面锁屏壁纸自动换", PipeDirection.Out)
			命名管道客户端流.Connect(1000)
			Select Case Command()
				Case "自启动"
					命名管道客户端流.WriteByte(启动类型.自启动)
				Case "换桌面"
					命名管道客户端流.WriteByte(启动类型.换桌面)
				Case "换锁屏"
					命名管道客户端流.WriteByte(启动类型.换锁屏)
				Case Else
					命名管道客户端流.WriteByte(启动类型.用户启动)
			End Select
			'延迟ShutDown，否则会在事件查看器中出现无意义的报错说ShutDownMode不能在关闭时设置
			应该ShutDown = True
			Exit Sub
		End Try
		命名管道服务器流.BeginWaitForConnection(AddressOf 管道回调, Nothing)
	End Sub
	Sub 唤醒()

	End Sub
	ReadOnly 下次唤醒 As New Timer(AddressOf 唤醒)
	Private Sub 当前窗口_Closed(sender As Object, e As EventArgs) Handles 当前窗口.Closed
		当前窗口 = Nothing
		Dim 时间阈值 As Date = Now + TimeSpan.FromHours(12)
		Dim 下次唤醒时间 As Date = 时间阈值
		For Each 子键 As RegistryKey In (From 键名 As String In 注册表根.GetSubKeyNames Select 注册表根.OpenSubKey(键名))
			Dim 本键轮换周期 As 轮换周期
			Select Case 子键.GetValue("有效")
				Case Nothing
					本键轮换周期 = 子键.GetValue("轮换周期", 轮换周期.禁用)
				Case True
					本键轮换周期 = 子键.GetValue("轮换周期", 默认桌面.GetValue("轮换周期", 轮换周期.禁用))
				Case False
					Continue For
			End Select
			If 本键轮换周期 = 轮换周期.禁用 Then
				Continue For
			End If
			Dim 下次更换时间 As Date = CDate(子键.GetValue("上次时间", Date.MinValue)) + 轮换周期转时间跨度(本键轮换周期)
			If 下次唤醒时间 > 下次更换时间 Then
				下次唤醒时间 = 下次更换时间
			End If
		Next
		If 下次唤醒时间 < 时间阈值 Then
			下次唤醒.Change(下次唤醒时间 - Now, Timeout.InfiniteTimeSpan)
		Else

		End If
	End Sub

	Private 日志文件 As StorageFile
	Friend 日志路径 As String
	Private 日志流 As StreamWriter

	Sub 消息(内容 As String)
		日志流.WriteLine($"{Now} {内容}")
	End Sub

	Sub 报错(异常 As Exception)
		日志流.WriteLine($"{Now} {异常.GetType} {异常.Message}")
	End Sub

	Friend 开机启动 As StartupTask

	Private Async Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
		If 应该ShutDown Then
			Shutdown()
			Exit Sub
		End If

		'这一行必须用打包项目启动UWP框架才能执行，纯WPF框架不支持此API
		日志文件 = Await ApplicationData.Current.TemporaryFolder.CreateFileAsync("日志.log", CreationCollisionOption.OpenIfExists)

		日志路径 = 日志文件.Path
		Dim 随机访问流 As IRandomAccessStream = Await 日志文件.OpenAsync(FileAccessMode.ReadWrite)
		随机访问流.Seek(随机访问流.Size)
		日志流 = New StreamWriter(随机访问流.AsStreamForWrite) With {.AutoFlush = True}
		开机启动 = Await StartupTask.GetAsync("自启动任务")
		Select Case Command()
			Case "自启动"
				自启动()
			Case "换桌面"
				自动换桌面()
				Shutdown()
			Case "换锁屏"
				Await 自动换锁屏()
				Shutdown()
			Case Else
				自启动()
				Call (New MainWindow).Show()
		End Select
	End Sub
End Class
