Imports Microsoft.Win32
Imports 本地桌面锁屏壁纸自动换.My
Imports System.IO
Imports System.Windows.Forms

Class MainWindow

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		If Settings.当前桌面目录 = "" Then
			Settings.当前桌面目录 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\Windows\Themes")
		End If
		If Settings.当前锁屏目录 = "" Then
			Settings.当前锁屏目录 = Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), "Microsoft\Windows\SystemData", Security.Principal.WindowsIdentity.GetCurrent.User.Value, "ReadOnly")
		End If
		桌面_当前搜索路径.Text = Settings.当前桌面目录
		锁屏_当前搜索路径.Text = Settings.当前锁屏目录
		桌面_更换周期.SelectedIndex = Settings.桌面轮换周期
		锁屏_更换周期.SelectedIndex = Settings.锁屏轮换周期
		桌面_浏览图集.Content = Settings.所有桌面目录
		锁屏_浏览图集.Content = Settings.所有锁屏目录
		桌面壁纸列表.ItemsSource = 桌面锁屏取设.各屏壁纸()
		Dim 锁屏文件 As String = Nothing
		Dim 最新修改 As Date = Date.MinValue
		Try
			For Each 目录 As String In Directory.GetDirectories(Settings.当前锁屏目录)
				Dim 文件 As String = Path.Combine(目录, "LockScreen.jpg")
				If File.Exists(文件) Then
					Dim 修改 As Date = File.GetLastWriteTime(文件)
					If 修改 > 最新修改 Then
						锁屏文件 = 文件
						最新修改 = 修改
					End If
				End If
			Next
			If 锁屏文件 Is Nothing Then
				Throw New FileNotFoundException($"【{Settings.当前锁屏目录}】中没有找到锁屏图片")
			Else
				锁屏_当前图片.Source = New BitmapImage(New Uri(锁屏文件))
			End If
		Catch ex As Exception
			锁屏图片错误.Text = $"{ex.GetType} {vbCrLf & ex.Message}"
		End Try
	End Sub

	Private 文件浏览对话框 As New FolderBrowserDialog

	Private Sub 桌面_浏览_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览.Click
		Dim 对话框结果 As DialogResult = 文件浏览对话框.ShowDialog
		If 对话框结果 = Forms.DialogResult.OK AndAlso Directory.Exists(文件浏览对话框.SelectedPath) Then

		End If
	End Sub
End Class
