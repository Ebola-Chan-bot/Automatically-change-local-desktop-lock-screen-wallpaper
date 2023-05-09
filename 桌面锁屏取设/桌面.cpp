#include "pch.h"
#include "桌面.h"
#include<ShObjIdl.h>
namespace 桌面锁屏取设
{
	ref class 监视设备列表 :public IReadOnlyList<监视设备>,IEnumerator<监视设备>
	{
		Collections::IEnumerator^ GetEnumerator1()override = Collections::IEnumerable::GetEnumerator
		{
			return this;
		};
		IEnumerator<监视设备>^ GetEnumerator2()override = IEnumerable<监视设备>::GetEnumerator
		{
			return this;
		};
		int Count()override = IReadOnlyList<监视设备>::Count::get
		{
			
		};
		监视设备 default(int 索引)override = IReadOnlyList<监视设备>::default::get
		{
			监视设备 返回;
		返回.索引 = 索引;
		return 返回;
		};
		Object^ Current1()override = Collections::IEnumerator::Current::get;
		bool MoveNext()override = Collections::IEnumerator::MoveNext;
		void Reset()override = Collections::IEnumerator::Reset;
		监视设备 Current2()override = IEnumerator<监视设备>::Current::get;
	public:
		static IDesktopWallpaper* 桌面壁纸;
		static 监视设备列表^ 唯一对象;
		virtual ~监视设备列表();
	};
	IDesktopWallpaper* 监视设备列表::桌面壁纸 = []()
	{
		IDesktopWallpaper*
	}();
	IReadOnlyList<监视设备>^ 监视设备::所有监视设备()
	{
		return 监视设备列表::唯一对象;
	}
}