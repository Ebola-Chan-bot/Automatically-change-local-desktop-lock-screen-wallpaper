Imports System.IO
Imports Windows.Foundation
Imports Windows.Storage
Imports Windows.Storage.Streams

Friend Module 日志
	ReadOnly 日志文件 As IAsyncOperation(Of StorageFile) = ApplicationData.Current.TemporaryFolder.CreateFileAsync("日志.log", CreationCollisionOption.OpenIfExists)
	Friend ReadOnly 日志路径 As Task(Of String) = (Async Function() As Task(Of String) 
												   Return (Await 日志文件).Path
											   End Function)()
	ReadOnly 日志流任务 As Task(Of StreamWriter) = (Async Function() As Task(Of StreamWriter)
												   Dim 随机访问流 As IRandomAccessStream = Await (Await 日志文件).OpenAsync(FileAccessMode.ReadWrite)
												   随机访问流.Seek(随机访问流.Size)
												   Return New StreamWriter(随机访问流.AsStreamForWrite) With {.AutoFlush = True}
											   End Function)()

	Async Sub 消息(内容 As String)
		Call (Await 日志流任务).WriteLine($"{Now} {内容}")
	End Sub

	Async Sub 报错(异常 As Exception)
		Call (Await 日志流任务).WriteLine($"{Now} {异常.GetType} {异常.Message}")
	End Sub
End Module
