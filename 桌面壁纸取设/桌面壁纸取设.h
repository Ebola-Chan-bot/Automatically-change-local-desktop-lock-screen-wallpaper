#pragma once
#include<stdint.h>
#include<ShObjIdl.h>
namespace 桌面壁纸取设 {
	public enum class 桌面壁纸位置:uint8_t
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
		//获取系统的监视器之一。构造后应当检查监视器是否有效。
		监视器设备(uint8_t 监视器索引);
		~监视器设备();
		//与系统关联的监视器数。
		static uint8_t 监视器设备计数();
		//将墙纸切换到幻灯片放映中的下一个图像。
		void 下一个桌面背景(bool 向后);
		//显示矩形，按引用传入输出参数获取矩形，返回值指示监视器是否有效。
		bool 矩形(System::Drawing::Rectangle%返回矩形);
		//使用监视器前应当检查是否有效。也可以用获取矩形的方法检查有效性。
		bool 有效();
		property System::String^ 壁纸路径
		{
			System::String^ get();
			void set(System::String^);
		}
		property System::String^ 路径名称
		{
			System::String^ get();
		}
	};
	public ref struct 桌面壁纸
	{
		//禁用桌面背景时，将在其位置显示纯色。
		static void 禁用();
		//不显示图像或禁用桌面背景时在桌面上可见的颜色。当桌面壁纸未填满整个屏幕时，此颜色也用作边框。
		static property System::Drawing::Color 背景颜色
		{
			System::Drawing::Color get();
			void set(System::Drawing::Color);
		}
		static property 桌面壁纸位置 位置
		{
			桌面壁纸位置 get();
		}
		static property System::String^ 幻灯片目录
		{
			System::String^ get();
			void set(System::String^);
		}
		static property 桌面幻灯片显示状态 幻灯片状态
		{
			桌面幻灯片显示状态 get();
		}
		static property 幻灯片选项结构 幻灯片选项
		{
			幻灯片选项结构 get();
			void set(幻灯片选项结构);
		}
	};
	public enum class 轮换周期枚举
	{
		禁用,
		分钟1,
		分钟2,
		分钟5,
		分钟10,
		分钟15,
		分钟30,
		小时1,
		小时2,
		小时3,
		小时6,
		小时12,
		天1,
		天2,
		天4,
		周1,
		周2,
		月1
	};
	public value struct 配置文件
	{
		property 轮换周期枚举 轮换周期;
		property System::String^ 图集目录;
		static property 配置文件 默认桌面配置;
		static property 配置文件 默认锁屏配置;
	};
}
