Imports 本地桌面锁屏壁纸自动换.My
Imports System.IO
Imports System.Windows
Imports 桌面锁屏取设

Class MainWindow

	Private Sub 桌面壁纸列表_MouseLeftButtonUp() Handles 桌面壁纸列表.MouseLeftButtonUp
		Try
			桌面壁纸列表.ItemsSource = 各屏壁纸(Settings.当前桌面目录)
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Private Sub 锁屏_当前图片_MouseLeftButtonUp() Handles 锁屏_当前图片.MouseLeftButtonUp
		Try
			锁屏_当前图片.Source = New BitmapImage(New Uri(壁纸路径(Settings.当前锁屏目录)))
		Catch ex As Exception
			锁屏图片错误.Text = $"{ex.GetType} {ex.Message}"
		End Try
	End Sub

	Sub New()

		' 此调用是设计器所必需的。
		InitializeComponent()

		' 在 InitializeComponent() 调用之后添加任何初始化。
		If Settings.当前桌面目录 = "" Then
			Settings.当前桌面目录 = 桌面.默认搜索路径
		End If
		If Settings.当前锁屏目录 = "" Then
			Settings.当前锁屏目录 = 锁屏.默认搜索路径
		End If
		桌面_当前搜索路径.Text = Settings.当前桌面目录
		锁屏_当前搜索路径.Text = Settings.当前锁屏目录
		桌面壁纸列表_MouseLeftButtonUp()
		锁屏_当前图片_MouseLeftButtonUp()
		桌面_更换周期.SelectedIndex = Settings.桌面轮换周期
		锁屏_更换周期.SelectedIndex = Settings.锁屏轮换周期
		桌面_浏览图集.Content = Settings.所有桌面目录
		锁屏_浏览图集.Content = Settings.所有锁屏目录
		DirectCast(System.Windows.Application.Current, Application).当前窗口 = Me
	End Sub

	Private 目录浏览对话框 As New Forms.FolderBrowserDialog

	Private Sub 桌面_浏览_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览.Click
		If 目录浏览对话框.ShowDialog = Forms.DialogResult.OK AndAlso Directory.Exists(目录浏览对话框.SelectedPath) Then
			Try
				桌面壁纸列表.ItemsSource = 各屏壁纸(目录浏览对话框.SelectedPath)
			Catch ex As Exception
				桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
				Exit Sub
			End Try
			Settings.当前桌面目录 = 目录浏览对话框.SelectedPath
			桌面_当前搜索路径.Text = Settings.当前桌面目录
		End If
	End Sub

	Private Sub 锁屏_浏览_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_浏览.Click
		If 目录浏览对话框.ShowDialog = Forms.DialogResult.OK AndAlso Directory.Exists(目录浏览对话框.SelectedPath) Then
			Try
				锁屏_当前图片.Source = New BitmapImage(New Uri(壁纸路径(目录浏览对话框.SelectedPath)))
			Catch ex As Exception
				锁屏图片错误.Text = $"{ex.GetType} {ex.Message}"
				Exit Sub
			End Try
			Settings.当前锁屏目录 = 目录浏览对话框.SelectedPath
			锁屏_当前搜索路径.Text = Settings.当前锁屏目录
		End If
	End Sub

	Private Sub 桌面_重置默认_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_重置默认.Click
		Try
			桌面壁纸列表.ItemsSource = 各屏壁纸()
		Catch ex As Exception
			桌面图片错误.Text = $"{ex.GetType} {ex.Message}"
			Exit Sub
		End Try
		Settings.当前桌面目录 = 桌面.默认搜索路径
		桌面_当前搜索路径.Text = Settings.当前桌面目录
	End Sub

	Private Sub 锁屏_重置默认_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_重置默认.Click
		Try
			锁屏_当前图片.Source = New BitmapImage(New Uri(壁纸路径(Settings.当前锁屏目录)))
		Catch ex As Exception
			锁屏图片错误.Text = $"{ex.GetType} {ex.Message}"
			Exit Sub
		End Try
		Settings.当前锁屏目录 = 锁屏.默认搜索路径
		锁屏_当前搜索路径.Text = Settings.当前锁屏目录
	End Sub

	Private Sub 桌面_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 桌面_浏览图集.Click
		If 目录浏览对话框.ShowDialog = Forms.DialogResult.OK AndAlso Directory.Exists(目录浏览对话框.SelectedPath) Then
			Settings.所有桌面目录 = 目录浏览对话框.SelectedPath
			桌面_浏览图集.Content = Settings.当前锁屏目录
		End If
	End Sub

	Private Sub 锁屏_浏览图集_Click(sender As Object, e As RoutedEventArgs) Handles 锁屏_浏览图集.Click
		If 目录浏览对话框.ShowDialog = Forms.DialogResult.OK AndAlso Directory.Exists(目录浏览对话框.SelectedPath) Then
			Settings.所有锁屏目录 = 目录浏览对话框.SelectedPath
			锁屏_浏览图集.Content = Settings.当前锁屏目录
		End If
	End Sub

	Private Sub 桌面_更换周期_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles 桌面_更换周期.SelectionChanged

	End Sub
End Class
