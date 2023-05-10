Imports System.IO
Imports System.IO.Pipes
Imports System.Windows.Threading
Imports 本地桌面锁屏壁纸自动换.My

Class Application

	' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
	' can be handled in this file.

	Friend WithEvents 当前窗口 As MainWindow

	Sub New()
		Dim 命名管道服务器流 As NamedPipeServerStream
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
			Shutdown()
			Exit Sub
		End Try
		命名管道服务器流.BeginWaitForConnection(Sub()
											Select Case 命名管道服务器流.ReadByte()
												Case 启动类型.用户启动
													If 当前窗口 Is Nothing Then
														Dispatcher.Invoke(Sub() Call (New MainWindow).Show())
													Else
														当前窗口.Activate()
													End If
												Case 启动类型.自启动
													自启动()
												Case 启动类型.换桌面
													换桌面()
												Case 启动类型.换锁屏
													换锁屏()
											End Select
											命名管道服务器流.Disconnect()
										End Sub, Nothing)
	End Sub

	Private Sub 当前窗口_Closed(sender As Object, e As EventArgs) Handles 当前窗口.Closed
		当前窗口 = Nothing
		If (Settings.桌面轮换周期 = 轮换周期.禁用 OrElse Settings.桌面轮换周期 > 轮换周期.小时12) AndAlso (Settings.锁屏轮换周期 = 轮换周期.禁用 OrElse Settings.锁屏轮换周期 > 轮换周期.小时12) Then
			Shutdown()
		End If
	End Sub

	Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
		If e.Args.Any Then
			Select Case e.Args.First
				Case "自启动"
					自启动()
				Case "换桌面"
					换桌面()
				Case "换锁屏"
					换锁屏()
				Case Else
					Shutdown(1)
			End Select
		Else
			StartupUri = New Uri("MainWindow.xaml", UriKind.Relative)
		End If
	End Sub
End Class
