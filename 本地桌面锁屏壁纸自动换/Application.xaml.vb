Imports System.Threading

Class Application

	' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
	' can be handled in this file.

	Private 当前窗口 As MainWindow

	Sub New()
		Dim 有人吗 As New EventWaitHandle(False, EventResetMode.AutoReset, "Acldlw.有人吗"), 有人 As New EventWaitHandle(False, EventResetMode.AutoReset, "Acldlw.有人"), 没人 As New EventWaitHandle(False, EventResetMode.AutoReset)
		有人.Reset()
		有人吗.Set()
		Task.Run(Sub()
					 If 有人.WaitOne(1000) Then
						 Dispatcher.InvokeShutdown()
					 End If
					 If WaitHandle.WaitAny({没人, 有人}) Then
						 Dispatcher.InvokeShutdown()
					 End If
				 End Sub)
		有人吗.Reset()
		没人.Set()
		Task.Run(Sub()
					 Do
						 有人吗.WaitOne()
						 有人.Set()
						 If IsNothing(当前窗口) Then
							 Dispatcher.Invoke(Sub() Call (New MainWindow).Show())
						 Else
							 Dispatcher.Invoke(AddressOf 当前窗口.Activate)
						 End If
					 Loop
				 End Sub)
	End Sub

	Friend Sub 窗口打开了(窗口 As MainWindow)
		当前窗口 = 窗口
		AddHandler 当前窗口.Closed, AddressOf 当前窗口_Closed
	End Sub

	Private Sub 当前窗口_Closed(sender As Object, e As EventArgs)
		当前窗口 = Nothing
		Static 非驻留计划 As 轮换周期() = {轮换周期.禁用, 轮换周期.}
		If 设置项.每隔时间单位 = 时间单位.关闭 OrElse 设置项.每隔时间单位 = 时间单位.天 Then
			Shutdown()
		End If
	End Sub

	Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
		If 设置项.保存后台日志 Then
			日志流 = IO.File.AppendText(设置项.后台日志路径)
		End If
		If e.Args.Any AndAlso e.Args.First = "后台任务" Then
			设置后台任务()
		Else
			StartupUri = New Uri("MainWindow.xaml", UriKind.Relative)
		End If
		写日志("启动成功")
	End Sub
End Class
