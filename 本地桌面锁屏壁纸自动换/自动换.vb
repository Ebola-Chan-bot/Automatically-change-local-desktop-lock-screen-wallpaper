Imports System.IO
Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports System.TimeSpan
Imports System.Threading
Imports Microsoft.Win32

Enum 轮换周期 As Byte
	禁用
	分钟1
	分钟2
	分钟5
	分钟10
	分钟15
	分钟30
	小时1
	小时2
	小时3
	小时6
	小时12
	天1
	天2
	天4
	周1
	周2
	月1
End Enum

Enum 启动类型 As Byte
	用户启动
	自启动
	换桌面
	换锁屏
End Enum

Module 自动换
	Friend ReadOnly 轮换周期转时间跨度 As TimeSpan() = {Timeout.InfiniteTimeSpan, FromMinutes(1), FromMinutes(2), FromMinutes(5), FromMinutes(10), FromMinutes(15), FromMinutes(30), FromHours(1), FromHours(2), FromHours(3), FromHours(6), FromHours(12)}

	ReadOnly 随机生成器 As New Random
	Friend ReadOnly Current As Application = System.Windows.Application.Current
	Friend Event 自动换_桌面()

	Sub 换桌面()
		Dim 所有图片 As String()
		Try
			所有图片 = Directory.GetFiles(Settings.所有桌面目录)
		Catch ex As ArgumentException
			Throw New ArgumentException("桌面图集目录无效", ex.ParamName, ex.InnerException)
		End Try
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		For a As Byte = 0 To 监视器个数
			Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
			Dim 监视器 As New 监视器设备(a)
			If 监视器.有效 Then
				监视器.壁纸路径 = 壁纸路径
				Current.消息($"{监视器.路径名称} 设置桌面 {壁纸路径}")
			End If
		Next
		Settings.上次桌面时间 = Now
		RaiseEvent 自动换_桌面()
	End Sub

	Friend Event 自动换_锁屏()
	ReadOnly 锁屏模式设置命令 As New ProcessStartInfo With {.FileName = "powershell.exe", .Arguments = "[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager',$true).SetValue('RotatingLockScreenEnabled', 0)", .CreateNoWindow = True}

	'必须返回Task才能捕获异常
	Async Function 换锁屏() As Task
		Dim 所有图片 As String()
		Try
			所有图片 = Directory.GetFiles(Settings.所有锁屏目录)
		Catch ex As ArgumentException
			Throw New ArgumentException("锁屏图集目录无效", ex.ParamName, ex.InnerException)
		End Try
		Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
		Process.Start(锁屏模式设置命令)
		Await Windows.System.UserProfile.LockScreen.SetImageFileAsync(Await Windows.Storage.StorageFile.GetFileFromPathAsync(壁纸路径))
		Current.消息($"设置锁屏 {壁纸路径}")
		Settings.上次锁屏时间 = Now
		RaiseEvent 自动换_锁屏()
	End Function

	Sub 自动换桌面()
		System.Windows.Application.Current.Dispatcher.Invoke(Sub()
																 Try
																	 换桌面()
																 Catch ex As Exception
																	 Current.报错(ex)
																 End Try
															 End Sub)
	End Sub

	Async Function 自动换锁屏() As Task
		Await System.Windows.Application.Current.Dispatcher.Invoke(Async Function() As Task
																	   Try
																		   Await 换锁屏()
																	   Catch ex As Exception
																		   Current.报错(ex)
																	   End Try
																   End Function)
	End Function

	Friend ReadOnly 桌面定时器 As New Timer(AddressOf 自动换桌面)
	Friend ReadOnly 锁屏定时器 As New Timer(AddressOf 自动换锁屏)

	Function 剩余时间(上次时间 As Date, 时间跨度 As TimeSpan) As TimeSpan
		If 时间跨度 = Timeout.InfiniteTimeSpan Then
			Return Timeout.InfiniteTimeSpan
		Else
			Dim 返回值 As TimeSpan = 上次时间 + 时间跨度 - Now
			Return If(返回值 > Zero, 返回值, Zero)
		End If
	End Function

	Sub 自启动()
		If Settings.桌面轮换周期 < 轮换周期.天1 Then
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.桌面轮换周期)
			桌面定时器.Change(剩余时间(Settings.上次桌面时间, 时间跨度), 时间跨度)
		End If
		If Settings.锁屏轮换周期 < 轮换周期.天1 Then
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.锁屏轮换周期)
			锁屏定时器.Change(剩余时间(Settings.上次锁屏时间, 时间跨度), 时间跨度)
		End If
	End Sub

	Sub 更换周期(桌面锁屏 As String, 桌面锁屏轮换周期 As (Byte, Byte), 定时器 As Timer, 上次桌面锁屏时间 As Date)
		Static 任务服务 As TaskScheduler.TaskService = TaskScheduler.TaskService.Instance
		Static 轮换周期转触发器 As TaskScheduler.Trigger() = {New TaskScheduler.DailyTrigger(1), New TaskScheduler.DailyTrigger(2), New TaskScheduler.DailyTrigger(4), New TaskScheduler.WeeklyTrigger(1), New TaskScheduler.WeeklyTrigger(2), New TaskScheduler.MonthlyTrigger(1)}
		Static 启动路径 As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\WindowsApps\本地桌面锁屏壁纸自动换.exe")
		Dim 计划任务 As TaskScheduler.Task = 任务服务.GetTask($"本地{桌面锁屏}自动换")
		If 桌面锁屏轮换周期.Item1 = 轮换周期.禁用 Then
			定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If 桌面锁屏轮换周期.Item2 > 轮换周期.小时12 OrElse 桌面锁屏轮换周期.Item2 = 轮换周期.禁用 Then
				Current.开机启动.Disable()
			End If
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
		ElseIf 桌面锁屏轮换周期.Item1 < 轮换周期.天1 Then
			Call Current.开机启动.RequestEnableAsync()
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(桌面锁屏轮换周期.Item1)
			定时器.Change(剩余时间(上次桌面锁屏时间, 时间跨度), 时间跨度)
		Else
			定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If 桌面锁屏轮换周期.Item2 > 轮换周期.小时12 OrElse 桌面锁屏轮换周期.Item2 = 轮换周期.禁用 Then
				Current.开机启动.Disable()
			End If
			Dim 触发器 As TaskScheduler.Trigger = 轮换周期转触发器(桌面锁屏轮换周期.Item1 - 轮换周期.天1).Clone
			If 计划任务 Is Nothing Then
				计划任务 = 任务服务.AddTask($"本地{桌面锁屏}自动换", 触发器, New TaskScheduler.ExecAction(启动路径, $"换{桌面锁屏}"))
				With 计划任务.Definition.Settings
					.StartWhenAvailable = True
					.DisallowStartIfOnBatteries = False
					.StopIfGoingOnBatteries = False
					.WakeToRun = True
					.IdleSettings.StopOnIdleEnd = False
					.RestartInterval = FromHours(2)
					.RestartCount = 11
				End With
			Else
				计划任务.Definition.Triggers.Item(0) = 触发器
			End If
			计划任务.RegisterChanges()
			计划任务.Enabled = True
		End If
	End Sub
End Module