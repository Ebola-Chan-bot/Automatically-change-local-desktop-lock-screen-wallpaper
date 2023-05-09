#include "pch.h"
#include "桌面.h"
#include<winrt/base.h>
#include<msclr/marshal_cppstd.h>
#pragma comment(lib,"shell32.lib")
#pragma comment(lib,"ole32.lib")
namespace 桌面锁屏取设
{
	IDesktopWallpaper* const 接口 = []()
	{
		IDesktopWallpaper* 返回值;
		CoCreateInstance(CLSID_DesktopWallpaper, NULL, CLSCTX_ALL, IID_IDesktopWallpaper, (LPVOID*)&返回值);
		return 返回值;
	}();
	监视器设备::监视器设备(uint8_t 监视器索引)
	{
		LPWSTR 返回值;
		接口->GetMonitorDevicePathAt(监视器索引, &返回值);
		监视器ID = 返回值;
	}
	监视器设备::~监视器设备()
	{
		CoTaskMemFree((LPVOID)监视器ID);
	}
	uint8_t 监视器设备::监视器设备计数()
	{
		UINT 返回值;
		接口->GetMonitorDevicePathCount(&返回值);
		return 返回值;
	}
	void 监视器设备::下一个桌面背景(bool 向后)
	{
		接口->AdvanceSlideshow(监视器ID, (DESKTOP_SLIDESHOW_DIRECTION)向后);
	}
	Drawing::Rectangle 监视器设备::矩形()
	{
		RECT 返回值;
		接口->GetMonitorRECT(监视器ID, &返回值);
		return Drawing::Rectangle::FromLTRB(返回值.left, 返回值.top, 返回值.right, 返回值.bottom);
	}
	String^ 监视器设备::壁纸路径::get()
	{
		LPWSTR 壁纸;
		接口->GetWallpaper(监视器ID, &壁纸);
		String^ 返回值 = gcnew String(壁纸);
		CoTaskMemFree(壁纸);
		return 返回值;
	}
	void 监视器设备::壁纸路径::set(String^新值)
	{
		msclr::interop::marshal_context 封送上下文;
		接口->SetWallpaper(监视器ID, 封送上下文.marshal_as<LPCWSTR>(新值));
	}
	void 桌面壁纸::禁用()
	{
		接口->Enable(false);
	}
	Drawing::Color 桌面壁纸::背景颜色::get()
	{
		COLORREF 颜色引用;
		接口->GetBackgroundColor(&颜色引用);
		return Drawing::Color::FromArgb(颜色引用);
	}
	void 桌面壁纸::背景颜色::set(Drawing::Color 新值)
	{
		接口->SetBackgroundColor(新值.ToArgb());
	}
	桌面壁纸位置 桌面壁纸::位置()
	{
		DESKTOP_WALLPAPER_POSITION 返回值;
		接口->GetPosition(&返回值);
		return (桌面壁纸位置)返回值;
	}
	String^ 桌面壁纸::幻灯片目录::get()
	{
		winrt::com_ptr<IShellItemArray>外壳项目数组;
		接口->GetSlideshow(外壳项目数组.put());
		winrt::com_ptr<IShellItem>外壳项目;
		外壳项目数组->GetItemAt(0, 外壳项目.put());
		LPWSTR 路径;
		外壳项目->GetDisplayName(SIGDN_FILESYSPATH, &路径);
		String^ 返回值 = gcnew String(路径);
		CoTaskMemFree(路径);
		return 返回值;
	}
	void 桌面壁纸::幻灯片目录::set(String^ 新值)
	{
		msclr::interop::marshal_context 封送上下文;
		winrt::com_ptr<IShellItem>外壳项目;
		SHCreateItemFromParsingName(封送上下文.marshal_as<PCWSTR>(新值), nullptr, IID_PPV_ARGS(外壳项目.put()));
		winrt::com_ptr<IShellItemArray>外壳项目数组;
		SHCreateShellItemArrayFromShellItem(外壳项目.get(), IID_PPV_ARGS(外壳项目数组.put()));
		接口->SetSlideshow(外壳项目数组.get());
	}
	桌面幻灯片显示状态 桌面壁纸::幻灯片状态()
	{
		桌面幻灯片显示状态 状态;
		接口->GetStatus(&状态.状态);
		return 状态;
	}
	幻灯片选项结构 桌面壁纸::幻灯片选项::get()
	{
		DESKTOP_SLIDESHOW_OPTIONS 桌面幻灯片选项;
		UINT 幻灯片放映间隔毫秒;
		接口->GetSlideshowOptions(&桌面幻灯片选项, &幻灯片放映间隔毫秒);
		幻灯片选项结构 返回值;
		返回值.扰乱图片顺序 = 桌面幻灯片选项;
		返回值.图片切换周期毫秒数 = 幻灯片放映间隔毫秒;
		return 返回值;
	}
	void 桌面壁纸::幻灯片选项::set(幻灯片选项结构 新值)
	{
		接口->SetSlideshowOptions((DESKTOP_SLIDESHOW_OPTIONS)新值.扰乱图片顺序, 新值.图片切换周期毫秒数);
	}
}