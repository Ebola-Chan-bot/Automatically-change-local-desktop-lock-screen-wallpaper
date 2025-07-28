Imports System.IO
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
	Function 更换单个监视器的桌面(注册表键 As RegistryKey) As String
		Dim 所有图片 As String()
		Dim 图集目录 As String = If(注册表键.GetValue("图集目录"), 注册表根.GetValue("图集目录"))
		注册表键.SetValue("上次时间", Now)
		Try
			所有图片 = Directory.GetFiles(图集目录)
		Catch ex As ArgumentException
			Throw New ArgumentException("图集目录无效", ex.ParamName, ex.InnerException)
		End Try
		If Not 所有图片.Length Then
			Throw New ArgumentException("图集目录内没有图片")
		End If
		Dim 监视器 As New 监视器设备(注册表键.Name)
		If 监视器.有效 Then
			Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
			监视器.壁纸路径 = 壁纸路径
			消息($"{注册表键.Name} 设置桌面 {壁纸路径}")
			Return 壁纸路径
		Else
			注册表键.SetValue("有效", False)
			Return Nothing
		End If
	End Function
	ReadOnly 锁屏模式设置命令 As New ProcessStartInfo With {.FileName = "powershell.exe", .Arguments = "[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager',$true).SetValue('RotatingLockScreenEnabled', 0)", .CreateNoWindow = True}
	Friend Event 自动换_锁屏(ex As Exception)

	'必须返回Task才能捕获异常。立即换锁屏并设置上次时间。
	Async Sub 换锁屏()
		Try
			Dim 所有图片 As String()
			Dim 图集目录 As String = 默认锁屏.GetValue("图集目录")
			Try
				所有图片 = Directory.GetFiles(图集目录)
			Catch ex As ArgumentException
				Throw New ArgumentException("锁屏图集目录无效", ex.ParamName, ex.InnerException)
			End Try
			If Not 所有图片.Length Then
				Throw New ArgumentException("锁屏图集目录没有图片")
			End If
			Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
			Process.Start(锁屏模式设置命令)
			Call Windows.System.UserProfile.LockScreen.SetImageFileAsync(Await StorageFile.GetFileFromPathAsync(壁纸路径))
			消息($"设置锁屏 {壁纸路径}")
			默认锁屏.SetValue("上次时间", Now)
			默认锁屏.SetValue("文件名", Path.GetFileName(壁纸路径))
		Catch ex As Exception
			报错(ex)
			RaiseEvent 自动换_锁屏(ex)
		End Try
		RaiseEvent 自动换_锁屏(Nothing)
	End Sub
	Function 检查更换() As TimeSpan
		检查更换 = Timeout.InfiniteTimeSpan
		Dim 现在 As Date = Now
		Dim 桌面换了 As Boolean = False
		For Each 键名 As String In 注册表根.GetSubKeyNames
			If 键名 = "桌面" Then Continue For
			Dim 本键轮换周期 As 轮换周期
			Dim 子键 As RegistryKey = 注册表根.OpenSubKey(键名)
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
			Dim 下次更换时间 As TimeSpan = If(本键轮换周期 = 轮换周期.月1, CDate(子键.GetValue("上次时间", Date.MinValue)).AddMonths(1), CDate(子键.GetValue("上次时间", Date.MinValue)) + 轮换周期转时间跨度(本键轮换周期)) - 现在
			If 下次更换时间 < FromMinutes(1) Then
				If 键名 = "锁屏" Then
					换锁屏()
				Else
					Dim 壁纸路径 As String
					Try
						壁纸路径 = 更换单个监视器的桌面(子键)
						If 壁纸路径 Is Nothing Then
							Continue For
						End If
						子键.SetValue("文件名", Path.GetFileName(壁纸路径))
						桌面换了 = True
					Catch ex As Exception
						报错(ex)
					End Try
				End If
				下次更换时间 = If(本键轮换周期 = 轮换周期.月1, CDate(子键.GetValue("上次时间", Date.MinValue)).AddMonths(1), CDate(子键.GetValue("上次时间", Date.MinValue)) + 轮换周期转时间跨度(本键轮换周期)) - 现在
			End If
			If 检查更换 > 下次更换时间 Then
				检查更换 = 下次更换时间
			End If
		Next
		If 桌面换了 Then
			RaiseEvent 自动换_桌面()
		End If
	End Function
	Friend 开机启动 As StartupTask
	Sub 保留或关闭(下次唤醒间隔 As TimeSpan)
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
	End Sub
	Sub 唤醒()
		If 当前窗口 Is Nothing Then
			保留或关闭(检查更换)
		Else
			检查更换设置唤醒()
		End If
	End Sub
	Friend ReadOnly 下次唤醒 As New Timer(AddressOf 唤醒)
	Sub 检查更换设置唤醒()
		Dim 下次更换间隔 As TimeSpan = 检查更换()
		下次唤醒.Change(下次更换间隔, 下次更换间隔)
	End Sub
End Module