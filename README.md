# phonedata port to CSharp


# It can download from Nuget.com now

	Install-Package Phonedata -Version 1.0.2
	
# Test Platform:

    Windows 7 x64
    vs 2017
    Net Framework v4.6.1

--------------------------------------------
# how to use it

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

## 手机号码库

#### 记录条数

387695 (updated: 2018年4月)

#### 其他语言支持

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

## License
#### [MIT](https://sndnvaps.mit-license.org/2018)
