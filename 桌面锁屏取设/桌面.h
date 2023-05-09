#pragma once
#include<stdint.h>
#include<ShObjIdl.h>
using namespace System;
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
	public value struct 幻灯片选项结构
	{
		property bool 扰乱图片顺序;
		property UINT 图片切换周期毫秒数;
	};
	public value struct 桌面幻灯片显示状态
	{
		bool 已启用() { return 状态 & DSS_ENABLED; }
		bool 已配置() { return 状态 & DSS_SLIDESHOW; }
		bool 被远程会话禁用() { return 状态 & DSS_DISABLED_BY_REMOTE_SESSION; };
	internal:
		DESKTOP_SLIDESHOW_STATE 状态;
	};
	public ref class 监视器设备
	{
		const wchar_t* 监视器ID;
	public:
		//获取系统的监视器之一
		监视器设备(uint8_t 监视器索引);
		~监视器设备();
		//与系统关联的监视器数。
		static uint8_t 监视器设备计数();
		//将墙纸切换到幻灯片放映中的下一个图像。
		void 下一个桌面背景(bool 向后);
		//显示矩形。
		Drawing::Rectangle 矩形();
		property String^ 壁纸路径
		{
			String^ get();
			void set(String^);
		}
	};
	public ref struct 桌面壁纸
	{
		//禁用桌面背景时，将在其位置显示纯色。
		static void 禁用();
		//不显示图像或禁用桌面背景时在桌面上可见的颜色。当桌面壁纸未填满整个屏幕时，此颜色也用作边框。
		static property Drawing::Color 背景颜色
		{
			Drawing::Color get();
			void set(Drawing::Color);
		}
		static 桌面壁纸位置 位置();
		static property String^ 幻灯片目录
		{
			String^ get();
			void set(String^);
		}
		static 桌面幻灯片显示状态 幻灯片状态();
		static property 幻灯片选项结构 幻灯片选项
		{
			幻灯片选项结构 get();
			void set(幻灯片选项结构);
		}
	};
}
