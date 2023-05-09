#pragma once
using namespace System;
namespace 桌面锁屏取设
{
	public ref struct 锁屏
	{
		static property String^ 图像文件
		{
			String^ get();
			void set(String^);
		}
	};
}