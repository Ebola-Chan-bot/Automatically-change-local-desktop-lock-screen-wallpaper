#include "pch.h"
#include "桌面壁纸取设.h"
#include<winrt/base.h>
#include<msclr/marshal.h>
#include<vector>
#include<序列化内存模型.hpp>
#include<winrt/windows.storage.h>
#pragma comment(lib,"shell32.lib")
#pragma comment(lib,"ole32.lib")
inline void COM异常检查(HRESULT 结果)
{
	if (FAILED(结果))
		System::Runtime::InteropServices::Marshal::ThrowExceptionForHR(结果);
}
IDesktopWallpaper* const 接口 = []()
{
	IDesktopWallpaper* 返回值;
	COM异常检查(CoCreateInstance(CLSID_DesktopWallpaper, NULL, CLSCTX_ALL, IID_IDesktopWallpaper, (LPVOID*)&返回值));
	return 返回值;
}();
struct 文件分配器
{
	const HANDLE 文件句柄;
	HANDLE 映射句柄;
	文件分配器(LPCWSTR 文件路径) :文件句柄(CreateFileW(文件路径, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL))
	{

	}
};
namespace 桌面壁纸取设
{
	监视器设备::监视器设备(uint8_t 监视器索引)
	{
		LPWSTR 返回值;
		COM异常检查(接口->GetMonitorDevicePathAt(监视器索引, &返回值));
		监视器ID = 返回值;
	}
	监视器设备::~监视器设备()
	{
		CoTaskMemFree((LPVOID)监视器ID);
	}
	uint8_t 监视器设备::监视器设备计数()
	{
		UINT 返回值;
		COM异常检查(接口->GetMonitorDevicePathCount(&返回值));
		return 返回值;
	}
	void 监视器设备::下一个桌面背景(bool 向后)
	{
		COM异常检查(接口->AdvanceSlideshow(监视器ID, (DESKTOP_SLIDESHOW_DIRECTION)向后));
	}
	bool 监视器设备::矩形(System::Drawing::Rectangle% NET返回值)
	{
		RECT COM返回值;
		const HRESULT 结果 = 接口->GetMonitorRECT(监视器ID, &COM返回值);
		NET返回值 = System::Drawing::Rectangle::FromLTRB(COM返回值.left, COM返回值.top, COM返回值.right, COM返回值.bottom);
		return SUCCEEDED(结果);
	}
	bool 监视器设备::有效()
	{
		RECT COM返回值;
		return SUCCEEDED(接口->GetMonitorRECT(监视器ID, &COM返回值));
	}
	System::String^ 监视器设备::壁纸路径::get()
	{
		LPWSTR 壁纸;
		COM异常检查(接口->GetWallpaper(监视器ID, &壁纸));
		System::String^ 返回值 = gcnew System::String(壁纸);
		CoTaskMemFree(壁纸);
		return 返回值;
	}
	void 监视器设备::壁纸路径::set(System::String^新值)
	{
		msclr::interop::marshal_context 封送上下文;
		COM异常检查(接口->SetWallpaper(监视器ID, 封送上下文.marshal_as<LPCWSTR>(新值)));
	}
	System::String^ 监视器设备::路径名称::get()
	{
		return gcnew System::String(监视器ID);
	}
	void 桌面壁纸::禁用()
	{
		COM异常检查(接口->Enable(false));
	}
	System::Drawing::Color 桌面壁纸::背景颜色::get()
	{
		COLORREF 颜色引用;
		COM异常检查(接口->GetBackgroundColor(&颜色引用));
		return System::Drawing::Color::FromArgb(颜色引用);
	}
	void 桌面壁纸::背景颜色::set(System::Drawing::Color 新值)
	{
		COM异常检查(接口->SetBackgroundColor(新值.ToArgb()));
	}
	桌面壁纸位置 桌面壁纸::位置::get()
	{
		DESKTOP_WALLPAPER_POSITION 返回值;
		COM异常检查(接口->GetPosition(&返回值));
		return (桌面壁纸位置)返回值;
	}
	System::String^ 桌面壁纸::幻灯片目录::get()
	{
		winrt::com_ptr<IShellItemArray>外壳项目数组;
		COM异常检查(接口->GetSlideshow(外壳项目数组.put()));
		winrt::com_ptr<IShellItem>外壳项目;
		COM异常检查(外壳项目数组->GetItemAt(0, 外壳项目.put()));
		LPWSTR 路径;
		COM异常检查(外壳项目->GetDisplayName(SIGDN_FILESYSPATH, &路径));
		System::String^ 返回值 = gcnew System::String(路径);
		CoTaskMemFree(路径);
		return 返回值;
	}
	void 桌面壁纸::幻灯片目录::set(System::String^ 新值)
	{
		msclr::interop::marshal_context 封送上下文;
		winrt::com_ptr<IShellItem>外壳项目;
		COM异常检查(SHCreateItemFromParsingName(封送上下文.marshal_as<PCWSTR>(新值), nullptr, IID_PPV_ARGS(外壳项目.put())));
		winrt::com_ptr<IShellItemArray>外壳项目数组;
		COM异常检查(SHCreateShellItemArrayFromShellItem(外壳项目.get(), IID_PPV_ARGS(外壳项目数组.put())));
		COM异常检查(接口->SetSlideshow(外壳项目数组.get()));
	}
	桌面幻灯片显示状态 桌面壁纸::幻灯片状态::get()
	{
		桌面幻灯片显示状态 状态;
		COM异常检查(接口->GetStatus(&状态.状态));
		return 状态;
	}
	幻灯片选项结构 桌面壁纸::幻灯片选项::get()
	{
		DESKTOP_SLIDESHOW_OPTIONS 桌面幻灯片选项;
		UINT 幻灯片放映间隔毫秒;
		COM异常检查(接口->GetSlideshowOptions(&桌面幻灯片选项, &幻灯片放映间隔毫秒));
		幻灯片选项结构 返回值;
		返回值.扰乱图片顺序 = 桌面幻灯片选项;
		返回值.图片切换周期毫秒数 = 幻灯片放映间隔毫秒;
		return 返回值;
	}
	void 桌面壁纸::幻灯片选项::set(幻灯片选项结构 新值)
	{
		COM异常检查(接口->SetSlideshowOptions((DESKTOP_SLIDESHOW_OPTIONS)新值.扰乱图片顺序, 新值.图片切换周期毫秒数));
	}
	void 配置文件::初始化()
	{
		HANDLE 文件 = CreateFileW((winrt::Windows::Storage::ApplicationData::Current().LocalFolder().Path() + L"\\桌面锁屏自动换配置.bin").c_str(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
	}
}