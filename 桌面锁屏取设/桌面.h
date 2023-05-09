#pragma once
#include<stdint.h>
#include<string>
using namespace System;
using namespace System::Collections::Generic;
namespace 桌面锁屏取设 {
	public enum 桌面壁纸位置:uint8_t
	{
		居中 = 0,
		平铺 = 1,
		拉伸 = 2,
		适应 = 3,
		填充 = 4,
		跨区 = 5
	};
	public value struct 监视设备
	{
		String^ 路径名称();
		//监视设备的左上角和右下角的虚拟XY坐标。左上角坐标为(0,0)的是主要监视设备。
		Drawing::Rectangle 显示矩形();
		void 下一个桌面背景(bool 向后);
		//静态壁纸图片路径
		property String^ 壁纸
		{
			String^ get();
			void set(String^);
		}
		//检索所有监视设备，以便对每个设备执行单独操作
		static IReadOnlyList<监视设备>^ 所有监视设备();
	internal:
		uint8_t 索引;
	};
	public value struct 幻灯片选项结构
	{
		property bool 扰乱图片顺序;
		property uint32_t 图片切换周期毫秒数;
	};
	public ref struct 桌面
	{
		//禁用一切桌面图片和幻灯片，只显示纯色背景
		static void 禁用();
		//设置纯色背景颜色
		static property Drawing::Color 背景颜色
		{
			Drawing::Color get();
			void set(Drawing::Color);
		}
		//选择适合你的桌面图象？
		static property 桌面壁纸位置 位置
		{
			桌面壁纸位置 get();
			void set(桌面壁纸位置);
		}
		static property String^ 幻灯片目录
		{
			String^ get();
			void set(String^);
		}
		static property 幻灯片选项结构 幻灯片选项
		{
			幻灯片选项结构 get();
			void set(幻灯片选项结构);
		}
		static bool 幻灯片启用();
		static bool 幻灯片已配置();
		static bool 幻灯片被远程会话禁用();
		//只有一个监视设备时可以使用此快速属性；否则请检索所有监视设备
		static property String^ 壁纸
		{
			String^ get();
			void set(String^);
		}
		//纯静态类，不允许构造对象
		桌面() = delete;
	};
}
