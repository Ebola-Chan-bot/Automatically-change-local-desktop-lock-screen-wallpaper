﻿Imports System.IO
Imports System.Threading
Imports System.TimeSpan
Imports Microsoft.Win32
Imports Microsoft.Win32.TaskScheduler
Imports Windows.ApplicationModel
Imports Windows.Storage
Imports 桌面壁纸取设
Enum 轮换周期 As Byte
	禁用
	分钟1
	分钟2
	分钟3
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
	默认
End Enum
Class 监视器异常
	Inherits Exception
	Sub New(消息 As String, 监视器ID As String, Optional 异常值 As String = Nothing, Optional 内部异常 As Exception = Nothing)
		MyBase.New($"{消息} {监视器ID} {异常值}", 内部异常)
	End Sub
End Class
Module 核心逻辑
	Friend 日志文件 As StorageFile
	Friend 日志路径 As String
	Friend 日志流 As StreamWriter
	Sub 消息(内容 As String)
		日志流.WriteLine($"{Now} {内容}")
	End Sub
	Function 报错(异常 As Exception) As String
		报错 = $"{Now} {异常.GetType} {异常.Message}"
		日志流.WriteLine(报错)
	End Function
	Sub 报错(自定义 As String)
		日志流.WriteLine($"{Now} {自定义}")
	End Sub
	Friend 当前窗口 As MainWindow
	Friend ReadOnly 注册表根 As RegistryKey = Registry.CurrentUser.CreateSubKey("Software\本地桌面锁屏壁纸自动换")
	Friend ReadOnly 默认桌面 As RegistryKey = 注册表根.CreateSubKey("桌面")
	Friend ReadOnly 默认锁屏 As RegistryKey = 注册表根.CreateSubKey("锁屏")
	Friend 轮换周期转时间跨度 As TimeSpan() = {Timeout.InfiniteTimeSpan, FromMinutes(1), FromMinutes(2), FromMinutes(3), FromMinutes(5), FromMinutes(10), FromMinutes(15), FromMinutes(30), FromHours(1), FromHours(2), FromHours(3), FromHours(6), FromHours(12), FromDays(1), FromDays(2), FromDays(4), FromDays(7), FromDays(14), FromDays(30)}

	Friend ReadOnly 随机生成器 As New Random
	Friend ReadOnly Current As Application = System.Windows.Application.Current
	Friend Event 自动换_桌面()
	Friend Event 自动换_锁屏(异常消息 As String)
	ReadOnly ContentDeliveryManager As RegistryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager")
	'必须返回Task才能捕获异常。立即换锁屏并设置上次时间。
	Async Sub 换锁屏()
		Try
			Dim 所有图片 As String()
			Dim 图集目录 As String = 默认锁屏.GetValue("图集目录")
			Try
				所有图片 = Directory.GetFiles(图集目录)
			Catch ex As ArgumentException
				Throw New 监视器异常("图集目录无效", "锁屏", 图集目录, ex)
			End Try
			If 所有图片.Length = 0 Then
				Throw New 监视器异常("图集目录没有图片", "锁屏", 图集目录)
			End If
			Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
			ContentDeliveryManager.SetValue("RotatingLockScreenEnabled", 0, RegistryValueKind.DWord)

			'必须等待，否则不会及时更改界面中的锁屏壁纸
			Await Windows.System.UserProfile.LockScreen.SetImageFileAsync(Await StorageFile.GetFileFromPathAsync(壁纸路径))

			消息($"设置锁屏 {壁纸路径}")
			默认锁屏.SetValue("文件名", Path.GetFileName(壁纸路径))
			默认锁屏.SetValue("上次时间", Now)
			RaiseEvent 自动换_锁屏(Nothing)
		Catch ex As Exception
			RaiseEvent 自动换_锁屏(报错(ex))
		End Try
	End Sub
	Function 检查更换() As TimeSpan
		检查更换 = MaxValue '不能用Timeout.InfiniteTimeSpan，因为该值是-1，不大于正常的TimeSpan值。
		Dim 本键轮换周期 As 轮换周期 = 默认锁屏.GetValue("更换周期", 轮换周期.禁用)
		Dim 现在 As Date = Now
		If 本键轮换周期 <> 轮换周期.禁用 Then
			Dim 上次时间 As Date = 默认锁屏.GetValue("上次时间", Date.MinValue)
			Dim 下次更换时间 As TimeSpan = If(本键轮换周期 = 轮换周期.月1, 上次时间.AddMonths(1), 上次时间 + 轮换周期转时间跨度(本键轮换周期)) - 现在
			If 下次更换时间 < FromSeconds(30) Then
				换锁屏()
				下次更换时间 = If(本键轮换周期 = 轮换周期.月1, 现在.AddMonths(1) - 现在, 轮换周期转时间跨度(本键轮换周期))
			End If
			检查更换 = 下次更换时间
		End If
		Dim 桌面换了 As Boolean = False
		Dim 默认图集 As String() = Nothing
		Dim 默认周期 As 轮换周期 = 默认桌面.GetValue("更换周期", 轮换周期.禁用)
		For M As Byte = 0 To 监视器设备.监视器设备计数() - 1
			Dim 监视器 As New 监视器设备(M)
			If Not 监视器.有效 Then
				Continue For
			End If
			Dim 监视器ID = 监视器.路径名称
			Dim 键名 As String = Convert.ToBase64String(Text.Encoding.Unicode.GetBytes(监视器ID)).Replace("/"c, "-"c)
			Dim 子键 As RegistryKey = 注册表根.CreateSubKey(键名)
			本键轮换周期 = 子键.GetValue("更换周期", 默认周期)

			'这两个If不能改用 Select Case，因为默认周期也可能是禁用，那之后仍然应该 Continue For。
			If 本键轮换周期 = 轮换周期.默认 Then
				本键轮换周期 = 默认周期
			End If
			If 本键轮换周期 = 轮换周期.禁用 Then
				Continue For
			End If

			Dim 下次更换时间 As TimeSpan = If(本键轮换周期 = 轮换周期.月1, CDate(子键.GetValue("上次时间", Date.MinValue)).AddMonths(1), CDate(子键.GetValue("上次时间", Date.MinValue)) + 轮换周期转时间跨度(本键轮换周期)) - 现在
			If 下次更换时间 < FromSeconds(30) Then
				Try
					Dim 所有图片 As String()
					Dim 图集目录 As String = 子键.GetValue("图集目录")
					If IsNothing(图集目录) Then
						If 默认图集 Is Nothing Then
							Dim 默认图集目录 As String = 默认桌面.GetValue("图集目录")
							Try
								默认图集 = Directory.GetFiles(默认图集目录)
							Catch ex As ArgumentException
								Throw New 监视器异常("图集目录无效", 监视器ID, 默认图集目录, ex)
							End Try
						End If
						所有图片 = 默认图集
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
					Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
					监视器.壁纸路径 = 壁纸路径
					消息($"{监视器ID} 设置桌面 {壁纸路径}")
					子键.SetValue("上次时间", Now)
					子键.SetValue("文件名", Path.GetFileName(壁纸路径))
					桌面换了 = True
				Catch ex As Exception
					报错(ex)
				End Try
				下次更换时间 = If(本键轮换周期 = 轮换周期.月1, 现在.AddMonths(1) - 现在, 轮换周期转时间跨度(本键轮换周期))
			End If
			If 检查更换 > 下次更换时间 Then
				检查更换 = 下次更换时间
			End If
		Next
		If 检查更换 = MaxValue Then
			检查更换 = Timeout.InfiniteTimeSpan
		End If
		If 桌面换了 Then
			RaiseEvent 自动换_桌面()
		End If
	End Function
	Friend 开机启动 As StartupTask

	'需要规划下次唤醒的方法，只能有一个执行，否则会导致计划混乱。其中，保留或关闭必须执行，检查更换设置唤醒可以跳过。
	ReadOnly 定时独占 As New Object
	Sub 保留或关闭()
		Monitor.Enter(定时独占)
		Try
			Dim 下次唤醒间隔 As TimeSpan = 检查更换()
			Static 任务服务 As TaskService = TaskService.Instance
			Static 启动路径 As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\WindowsApps\桌面锁屏自动换.exe")
			Dim 任务名称 As String = "本地桌面锁屏自动换"
			Dim 计划任务 As Task = 任务服务.GetTask(任务名称)
			If 下次唤醒间隔 = Timeout.InfiniteTimeSpan Then
				开机启动.Disable()
				任务服务.RootFolder.DeleteTask(任务名称, False)
				Current.Shutdown()
			ElseIf 下次唤醒间隔 < FromHours(12) Then
				Call 开机启动.RequestEnableAsync()
				If 计划任务 IsNot Nothing Then
					计划任务.Enabled = False
				End If
				下次唤醒.Change(下次唤醒间隔, 下次唤醒间隔)
			Else
				开机启动.Disable()
				Dim 触发器 As Trigger = New DailyTrigger(Math.Round(下次唤醒间隔.TotalDays))
				If 计划任务 Is Nothing Then
					计划任务 = 任务服务.AddTask(任务名称, 触发器, New ExecAction(启动路径, "后台启动"))
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
				Current.Shutdown()
			End If
		Catch ex As Exception
			报错(ex)
		Finally
			Monitor.Exit(定时独占)
		End Try
	End Sub

	'调用的COM接口不支持多线程，必须在原来线程上调度
	Friend ReadOnly 下次唤醒 As New Timer(Sub() Current.Dispatcher.Invoke(Sub()
																		  If 当前窗口 Is Nothing Then
																			  保留或关闭()
																		  Else
																			  检查更换设置唤醒()
																		  End If
																	  End Sub))
	Sub 检查更换设置唤醒()
		If Monitor.TryEnter(定时独占) Then
			Try
				Dim 下次更换间隔 As TimeSpan = 检查更换()
				下次唤醒.Change(下次更换间隔, 下次更换间隔)
			Catch ex As Exception
				报错(ex)
			Finally
				Monitor.Exit(定时独占)
			End Try
		End If
	End Sub
End Module