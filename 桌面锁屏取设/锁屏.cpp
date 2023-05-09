#include "pch.h"
#include "锁屏.h"
#include<winrt/windows.system.userprofile.h>
#include<winrt/windows.storage.h>
#include<winrt/windows.foundation.h>
#include<msclr/marshal.h>
using namespace winrt::Windows::System::UserProfile;
namespace 桌面锁屏取设
{
	String^ 锁屏::图像文件::get()
	{
		winrt::hstring 路径 = LockScreen::OriginalImageFile().RawUri();
		return gcnew String(路径.c_str());
	}
	void 锁屏::图像文件::set(String^ 路径)
	{
		msclr::interop::marshal_context 封送上下文;
		LockScreen::SetImageFileAsync(winrt::Windows::Storage::StorageFile::GetFileFromPathAsync(封送上下文.marshal_as<wchar_t*>(路径)).GetResults());
	}
}