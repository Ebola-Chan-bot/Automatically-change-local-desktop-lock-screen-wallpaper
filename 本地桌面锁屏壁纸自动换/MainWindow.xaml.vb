Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports Windows.System.UserProfile
Imports System.Threading
Imports Windows.ApplicationModel
Imports Microsoft.Win32.TaskScheduler
Imports Windows.Storage

Class MainWindow

	Private Sub 桌面壁纸列表_MouseLeftButtonUp() Handles 桌面壁纸列表.MouseLeftButtonUp
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		Dim 有效监视器 As New List(Of 监视器设备)
		For a As Byte = 0 To 监视器个数
			Dim 新设备 As New 监视器设备(a)
			Dim 矩形 As System.Drawing.Rectangle = 新设备.矩形
			If 矩形.Width AndAlso 矩形.Height Then
				有效监视器.Add(新设备)
			End If
		Next
		Try
			桌面壁纸列表.ItemsSource = 有效监视器
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Sub 锁屏_当前图片_MouseLeftButtonUp() Handles 锁屏_当前图片.MouseLeftButtonUp
		Try
			锁屏_当前图片.Source = New BitmapImage(LockScreen.OriginalImageFile)
		Catch ex As Exception
			锁屏图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private 目录浏览对话框 As Pickers.FolderPicker = (Function()
												   Dim 返回值 As New Pickers.FolderPicker
												   返回值.FileTypeFilter.Add("*")
												   Return 返回值
											   End Function)()

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		桌面壁纸列表_MouseLeftButtonUp()
		锁屏_当前图片_MouseLeftButtonUp()
		桌面_更换周期.SelectedIndex = Settings.桌面轮换周期
		锁屏_更换周期.SelectedIndex = Settings.锁屏轮换周期
		桌面_图集目录.Text = Settings.所有桌面目录
		锁屏_图集目录.Text = Settings.所有锁屏目录
		DirectCast(System.Windows.Application.Current, Application).当前窗口 = Me
		AddHandler 桌面_更换周期.SelectionChanged, AddressOf 桌面_更换周期_SelectionChanged
		AddHandler 锁屏_更换周期.SelectionChanged, AddressOf 锁屏_更换周期_SelectionChanged
	End Sub

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
		换桌面()
	End Sub

	Private Sub 锁屏_立即更换_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_立即更换.Click
		换锁屏()
	End Sub

	ReadOnly 开机启动 As StartupTask = StartupTask.GetAsync("自启动任务").GetResults
	ReadOnly 任务服务 As TaskService = TaskService.Instance
	ReadOnly 轮换周期转触发器 As Trigger() = {New DailyTrigger(1), New DailyTrigger(2), New DailyTrigger(4), New WeeklyTrigger(1), New WeeklyTrigger(2), New MonthlyTrigger(1)}
	ReadOnly 启动路径 As String = Process.GetCurrentProcess.MainModule.FileName

	Private Sub 桌面_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		Static 计划任务 As Task
		Settings.桌面轮换周期 = 桌面_更换周期.SelectedIndex
		If Settings.桌面轮换周期 = 轮换周期.禁用 Then
			桌面定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If Settings.锁屏轮换周期 > 轮换周期.小时12 OrElse Settings.锁屏轮换周期 = 轮换周期.禁用 Then
				开机启动.Disable()
			End If
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
		ElseIf Settings.桌面轮换周期 < 轮换周期.天1 Then
			Call 开机启动.RequestEnableAsync()
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.桌面轮换周期)
			桌面定时器.Change(TimeSpanMin(时间跨度, Now - Settings.上次桌面时间), 时间跨度)
		Else
			桌面定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If Settings.锁屏轮换周期 > 轮换周期.小时12 OrElse Settings.锁屏轮换周期 = 轮换周期.禁用 Then
				开机启动.Disable()
			End If
			If IsNothing(计划任务) Then
				计划任务 = 任务服务.AddTask("本地桌面自动换", 轮换周期转触发器(Settings.桌面轮换周期 - 轮换周期.天1), New ExecAction(启动路径, "换桌面"))
			End If
			计划任务.Enabled = True
		End If
	End Sub

	Private Sub 锁屏_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
		Static 计划任务 As Task
		Settings.锁屏轮换周期 = 锁屏_更换周期.SelectedIndex
		If Settings.锁屏轮换周期 = 轮换周期.禁用 Then
			锁屏定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If Settings.桌面轮换周期 > 轮换周期.小时12 OrElse Settings.桌面轮换周期 = 轮换周期.禁用 Then
				开机启动.Disable()
			End If
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
		ElseIf Settings.锁屏轮换周期 < 轮换周期.天1 Then
			Call 开机启动.RequestEnableAsync()
			If 计划任务 IsNot Nothing Then
				计划任务.Enabled = False
			End If
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.锁屏轮换周期)
			锁屏定时器.Change(TimeSpanMin(时间跨度, Now - Settings.上次锁屏时间), 时间跨度)
		Else
			锁屏定时器.Change(Timeout.Infinite, Timeout.Infinite)
			If Settings.桌面轮换周期 > 轮换周期.小时12 OrElse Settings.桌面轮换周期 = 轮换周期.禁用 Then
				开机启动.Disable()
			End If
			If IsNothing(计划任务) Then
				计划任务 = 任务服务.AddTask("本地锁屏自动换", 轮换周期转触发器(Settings.锁屏轮换周期 - 轮换周期.天1), New ExecAction(启动路径, "换锁屏"))
			End If
			计划任务.Enabled = True
		End If
	End Sub
End Class
