Imports System.IO
Imports System.IO.Pipes
Imports System.Windows.Threading
Imports Windows.ApplicationModel
Imports Windows.Storage
Imports Windows.Storage.Streams
Class Application

	' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
	' can be handled in this file.
	Private 命名管道服务器流 As NamedPipeServerStream

	Private Sub 管道回调()
		If 当前窗口 Is Nothing Then
			Dispatcher.Invoke(Sub() Call (New MainWindow).Show())
		Else
			Dispatcher.Invoke(AddressOf 当前窗口.Activate)
		End If
		命名管道服务器流.Disconnect()
		命名管道服务器流.BeginWaitForConnection(AddressOf 管道回调, Nothing)
	End Sub

	Private Async Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
		'如果已有实例在运行，则激活那个实例，然后自己退出。
		Try
			命名管道服务器流 = New NamedPipeServerStream("本地桌面锁屏壁纸自动换", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous Or PipeOptions.CurrentUserOnly)
		Catch ex As IOException
			Dim 命名管道客户端流 As New NamedPipeClientStream(".", "本地桌面锁屏壁纸自动换", PipeDirection.Out)
			命名管道客户端流.Connect(1000)
			'不能在 Sub New() 中Shutdown，会被当作异常退出而产生崩溃报告。
			Shutdown()
			Exit Sub
		End Try
		命名管道服务器流.BeginWaitForConnection(AddressOf 管道回调, Nothing)

		'这一行必须用打包项目启动UWP框架才能执行，纯WPF框架不支持此API
		日志文件 = Await ApplicationData.Current.TemporaryFolder.CreateFileAsync("日志.log", CreationCollisionOption.OpenIfExists)

		日志路径 = 日志文件.Path
		Dim 随机访问流 As IRandomAccessStream = Await 日志文件.OpenAsync(FileAccessMode.ReadWrite)
		随机访问流.Seek(随机访问流.Size)
		日志流 = New StreamWriter(随机访问流.AsStreamForWrite) With {.AutoFlush = True}
		If Command() = "后台启动" Then
			保留或关闭()
		Else
			Call (New MainWindow).Show()
		End If
	End Sub
End Class
