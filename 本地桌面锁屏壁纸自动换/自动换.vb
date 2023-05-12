Imports System.IO
Imports 本地桌面锁屏壁纸自动换.My
Imports 桌面壁纸取设
Imports System.TimeSpan
Imports System.Threading

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

	Function TimeSpanMin(A As TimeSpan, B As TimeSpan) As TimeSpan
		Return If(A < B, A, B)
	End Function

	ReadOnly 随机生成器 As New Random

	Sub 换桌面()
		Dim 所有图片 As String() = Directory.GetFiles(Settings.所有桌面目录)
		Dim 监视器个数 As Byte = 监视器设备.监视器设备计数() - 1
		For a As Byte = 0 To 监视器个数
			Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
			Dim 监视器 As New 监视器设备(a) With {.壁纸路径 = 壁纸路径}
			消息($"{监视器.路径名称} 设置桌面 {壁纸路径}")
		Next
		Settings.上次桌面时间 = Now
	End Sub

	Async Sub 换锁屏()
		Dim 所有图片 As String() = Directory.GetFiles(Settings.所有桌面目录)
		Dim 壁纸路径 As String = 所有图片(随机生成器.Next(所有图片.Length))
		Call Windows.System.UserProfile.LockScreen.SetImageFileAsync(Await Windows.Storage.StorageFile.GetFileFromPathAsync(壁纸路径))
		消息($"设置锁屏 {壁纸路径}")
		Settings.上次锁屏时间 = Now
	End Sub

	Friend ReadOnly 桌面定时器 As New Timer(AddressOf 换桌面)
	Friend ReadOnly 锁屏定时器 As New Timer(AddressOf 换锁屏)

	Sub 自启动()
		If Settings.桌面轮换周期 < 轮换周期.天1 Then
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.桌面轮换周期)
			桌面定时器.Change(TimeSpanMin(时间跨度, Now - Settings.上次桌面时间), 时间跨度)
		End If
		If Settings.锁屏轮换周期 < 轮换周期.天1 Then
			Dim 时间跨度 As TimeSpan = 轮换周期转时间跨度(Settings.锁屏轮换周期)
			锁屏定时器.Change(TimeSpanMin(时间跨度, Now - Settings.上次锁屏时间), 时间跨度)
		End If
	End Sub
End Module