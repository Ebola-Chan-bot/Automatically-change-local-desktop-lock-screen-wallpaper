#pragma once
#include<stdint.h>
#include<filesystem>
using namespace System;

namespace 桌面锁屏取设 {
	public value struct 屏幕壁纸
	{
		property String^ 设备名称;
		property String^ 壁纸路径;
	};
	public enum 桌面壁纸样式 :uint8_t
	{
		填充 = 10,
		适应 = 6,
		拉伸 = 2,
		平铺 = 23,
		居中 = 0,
		跨区 = 22
	};
	public ref struct 桌面
	{
		static property String^ 默认搜索路径
		{
			String^ get();
		}
	};
}
