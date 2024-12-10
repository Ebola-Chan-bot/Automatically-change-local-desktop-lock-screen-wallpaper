Imports Windows.Storage
Imports Windows.Storage.Streams

Class 配置文件
	Property 图集目录 As String
	Property 轮换周期 As 轮换周期
	Public Shared ReadOnly 默认桌面配置 As New 配置文件(0)
	Public Shared ReadOnly 默认锁屏配置 As New 配置文件(1)
	Shared Function 从监视器名取得配置(监视器名 As String) As 配置文件

	End Function
	Shared ReadOnly 配置流 As Task(Of IRandomAccessStream) = (Async Function() Await (Await ApplicationData.Current.LocalFolder.CreateFileAsync("桌面锁屏自动换.埃博拉酱", CreationCollisionOption.OpenIfExists)).OpenAsync(FileAccessMode.ReadWrite))()
	ReadOnly 索引 As Byte
	Protected Sub New(索引 As Byte)
		Me.索引 = 索引
	End Sub
End Class