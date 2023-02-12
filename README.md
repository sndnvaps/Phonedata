# phonedata port to CSharp


# It can download from Nuget.com use Powershell

	Install-Package Phonedata -Version 1.1.0
	
# Test Platform:

    Windows 7 x64
    vs 2017
    Net Framework v4.6.1

--------------------------------------------


## 手机号码库

 - 归属地信息库文件大小：4,152,564 字节
 - 归属地信息库最后更新：2023年02月
 - 手机号段记录条数：460232

phone type沿用源数据的定义：

* 1: 移动
* 2: 联通
* 3: 电信
* 4: 电信虚拟运营商
* 5: 联通虚拟运营商
* 6: 移动虚拟运营商
* 7: 广电


## 其他语言实现
 
 go: https://github.com/xluohome/phonedata

 python: https://github.com/lovedboy/phone
 
 php :  https://github.com/shitoudev/phone-location , https://github.com/iwantofun/php_phone
 
 php ext: https://github.com/jonnywang/phone
 
 java: https://github.com/fengjiajie/phone-number-geo
 
 Node: https://github.com/conzi/phone
 
 C++: https://github.com/yanxijian/phonedata
 
 C#: https://github.com/sndnvaps/Phonedata

 Rust: https://github.com/vincascm/phonedata

 Kotlin: https://github.com/bytebeats/phone-geo

下载 [phone.dat](https://raw.githubusercontent.com/sndnvaps/Phonedata/master/phone.dat) 文件，用其他语言解析即可。


#### phone.dat文件格式

```

        | 4 bytes |                     <- phone.dat 版本号
        ------------
        | 4 bytes |                     <-  第一个索引的偏移
        -----------------------
        |  offset - 8            |      <-  记录区
        -----------------------
        |  index                 |      <-  索引区
        -----------------------

```

* `头部` 头部为8个字节，版本号为4个字节，第一个索引的偏移为4个字节。
* `记录区` 中每条记录的格式为"\<省份\>|\<城市\>|\<邮编\>|\<长途区号\>\0"。 每条记录以'\0'结束。  
* `索引区` 中每条记录的格式为"<手机号前七位><记录区的偏移><卡类型>"，每个索引的长度为9个字节。

解析步骤:

 * 解析头部8个字节，得到索引区的第一条索引的偏移。
 * 在索引区用二分查找得出手机号在记录区的记录偏移。
 * 在记录区从上一步得到的记录偏移处取数据，直到遇到'\0'。


# How to use it

    import phonedata.dll to you C# project after build from source code or just use Powershell to install it 
	
# example code

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phonedata;

namespace phonedatacmd
{
    class Program
    {


        static void Main(string[] args)
        {
            Phonedata.Phonedata pd = new Phonedata.Phonedata("phone.dat");
            string output;
            output = pd.Lookup("13822399111").ToString();
            Console.WriteLine(output);
        }
    }
}
```

## 安全保证

手机号归属地信息是通过网上公开数据进行收集整理。

对手机号归属地信息数据的绝对正确，我不做任何保证。因此在生产环境使用前请您自行校对测试。

-----
Buy me a beer

<a href="https://liberapay.com/sndnvaps/donate"><img alt="Donate using Liberapay" src="https://liberapay.com/assets/widgets/donate.svg"></a>

## License
#### [MIT](https://sndnvaps.mit-license.org/2018)
