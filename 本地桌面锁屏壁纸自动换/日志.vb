Imports System.IO
Imports Windows.Storage
Imports Windows.Storage.Streams

Friend Module 日志
	ReadOnly 日志流任务 As Task(Of StreamWriter) = Task.Run(Async Function()
														   Dim 随机访问流 As IRandomAccessStream = Await (Await ApplicationData.Current.TemporaryFolder.CreateFileAsync("日志.log", CreationCollisionOption.OpenIfExists)).OpenAsync(FileAccessMode.ReadWrite)
														   随机访问流.Seek(随机访问流.Size)
														   Return New StreamWriter(随机访问流.AsStreamForWrite) With {.AutoFlush = True}
													   End Function)

	Async Sub 消息(内容 As String)
		Call (Await 日志流任务).WriteLine($"{Now} {内容}")
	End Sub

	Async Sub 报错(异常 As Exception)
		Call (Await 日志流任务).WriteLine($"{Now} {异常.GetType} {异常.Message}")
	End Sub
End Module
