using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Phonedata
{
    public class PhoneData
    {
        private static string Version = "";  //版本号
        private static byte[] Buf; //存储所有的数据于byte[]中
        private static Dictionary<int, string> dicBuf;
        enum CardType
        {
            UNKNOWN = 0,    // 未知，查找失败
            CMCC,           // 中国移动
            CUCC,           // 中国联通
            CTCC,           // 中国电信
            CTCC_V,         // 电信虚拟运营商
            CUCC_V,         // 联通虚拟运营商
            CMCC_V			// 移动虚拟运营商
        };

        /// <summary>
        /// 手机号码信息结构体
        /// </summary>
        public struct PhoneRecord
        {
            public string PhoneNum;
            public string Province;
            public string City;
            public string ZipCode;
            public string AreaZon;
            public string CardType;

            /// <summary>
            /// 用于设置 PhoneRecord{} 结构体中的数据
            /// </summary>
            /// <param name="phoneNum"></param>
            /// <param name="province"></param>
            /// <param name="city"></param>
            /// <param name="zipCode"></param>
            /// <param name="areaZon"></param>
            /// <param name="cardType"></param>
            public void Set(string phoneNum, string province, string city, string zipCode, string areaZon, string cardType)
            {
                this.PhoneNum = phoneNum;
                this.Province = province;
                this.City = city;
                this.ZipCode = zipCode;
                this.AreaZon = areaZon;
                this.CardType = cardType;
            }

            /// <summary>
            /// 把 PhoneRecord{} 结构体中的数据，转换成string
            /// </summary>
            /// <returns></returns>
            override public string ToString()
            {
                string tmp = string.Format("PhoneNum: {0}\nAreaZon: {1}\nCardType: {2}\nCity: {3}\nZipCode: {4}\nProvince: {5}\n", this.PhoneNum, this.AreaZon, this.CardType, this.City, this.ZipCode, this.Province);
                return tmp;
            }

        };

        /// <summary>
        /// 手机号码结构体
        /// </summary>
        private struct PhoneInfo
        {
            public UInt32 Phone7;
            public string PhoneNum;
        };

        /// <summary>
        /// class phonedata 的构造函数
        /// </summary>
        public PhoneData()
        {
            string phone_dat_path = "phone.dat";
            Init(phone_dat_path);
        }

        /// <summary>
        /// class phonedata 的构造函数
        /// </summary>
        public PhoneData(string phonedata)
        {
            Init(phonedata);
        }

        private byte[] SubByte(byte[] srcBytes, int startIndex, int length)
        {
            System.IO.MemoryStream bufferStream = new System.IO.MemoryStream();

            // Console.WriteLine("srcbyte length = {0}\n", srcBytes.Length);

            byte[] returnByte = new byte[] { };
            if (srcBytes == null) { return returnByte; }
            if (startIndex < 0) { startIndex = 0; }
            if (startIndex < srcBytes.Length)
            {
                if (length < 1 || length > srcBytes.Length - startIndex) { length = srcBytes.Length - startIndex; }
                bufferStream.Write(srcBytes, startIndex, length);
                returnByte = bufferStream.ToArray();
                bufferStream.SetLength(0);
                bufferStream.Position = 0;
            }
            bufferStream.Close();
            bufferStream.Dispose();
            return returnByte;
        }

        //turn 4 byte to int32
        /// <summary>
        /// 将 4个byte转换成 int32类型
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private Int32 Get4(byte[] b)
        {
            Int32 tmp;
            if (b.Length < 4)
            {
                return 0;
            }
            tmp = (Int32)(b[0]) | ((Int32)(b[1])) << 8 | ((Int32)(b[2])) << 16 | ((Int32)(b[3])) << 24;
            return tmp;
        }

        /// <summary>
        /// 返回 手机号码的卡类别，比如：移动，电信，联通。。。
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private string GetCardType(byte b)
        {
            int type = Convert.ToInt32(b);
            string tmp = "";
            switch (type)
            {
                case (Int32)(CardType.CMCC):
                    tmp = "中国移动";
                    break;
                case (Int32)(CardType.CMCC_V):
                    tmp = "中国移动虚拟运营商";
                    break;
                case (Int32)(CardType.CTCC):
                    tmp = "中国电信";
                    break;
                case (Int32)(CardType.CTCC_V):
                    tmp = "中国电信虚拟运营商";
                    break;
                case (Int32)(CardType.CUCC):
                    tmp = "中国联通";
                    break;
                case (Int32)(CardType.CUCC_V):
                    tmp = "中国联通虚拟运营商";
                    break;
                default:
                    tmp = "未知电信运营商";
                    break;

            }
            return tmp;
        }

        /// <summary>
        /// 初始化程序
        /// </summary>
        /// <param name="phonedata"></param>
        public void Init(string phonedata)
        {
            dicBuf = new Dictionary<int, string>();
            byte[] tmp = new byte[4];

            if (!File.Exists(phonedata))
            {
                return;
            }

            byte[] zeroByte = Encoding.UTF8.GetBytes("\0");
            int boundary;//第一个索引区的引索
            int phone7;//电话号码前七位
            int phoneDataIndex;//电话号码属性引索
            int cardType;//卡类型值
            string propertyCare;//合并卡类型后的电话号码属性
            using (FileStream fs = new FileStream("phone.dat", FileMode.OpenOrCreate, FileAccess.Read))
            {
                Buf = new byte[fs.Length];
                fs.Read(Buf, 0, (int)fs.Length);
                boundary = Buf[4] | Buf[5] << 8 | Buf[6] << 16 | Buf[7] << 24;
                for (int i = boundary; i < Buf.Length; i += 9)
                {
                    phone7 = Buf[i] | Buf[i + 1] << 8 | Buf[i + 2] << 16 | Buf[i + 3] << 24;
                    phoneDataIndex = Buf[i + 4] | Buf[i + 5] << 8 | Buf[i + 6] << 16 | Buf[i + 7] << 24;
                    cardType = Buf[i + 8];
                    for (int n = phoneDataIndex; n < boundary; n++)
                    {
                        if (Buf[n] == zeroByte[0])
                        {
                            propertyCare = Encoding.UTF8.GetString(Buf, phoneDataIndex, n - phoneDataIndex) + "|" + cardType;
                            dicBuf.Add(phone7, propertyCare);
                            break;
                        }
                    }
                }
            }

            tmp = SubByte(Buf, 0, 4);
            Version = Encoding.Default.GetString(tmp);
        }

        /// <summary>
        /// 返回phone.dat的版本号
        /// </summary>
        /// <returns> 返回版本号</returns>
        public string GetVersion()
        {
            return Version;
        }

        /// <summary>
        /// 返回查找的号码的信息
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public PhoneRecord Lookup(Int64 phone)
        {
            PhoneInfo pi = new PhoneInfo { };
            pi.PhoneNum = phone.ToString();
            if (phone >= 1000000 && phone <= 99999999999)
            {
                while (phone > 9999999)
                {
                    phone /= 10;
                }
                pi.Phone7 = (UInt32)phone;

                return FindPhone(pi);
            }
            return new PhoneRecord { };
        }

        /// <summary>
        /// 返回查找的号码的信息
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public PhoneRecord Lookup(string phone)
        {
            long phone7;
            PhoneInfo pi = new PhoneInfo { };
            if (phone.Length >= 7 && phone.Length <= 11)
            {
                try
                {

                    phone7 = Convert.ToInt64(phone.Substring(0, 7));
                    if (phone7 >= 1000000 && phone7 <= 99999999999)
                    {
                        while (phone7 > 9999999)
                        {
                            phone7 /= 10;
                        }
                        pi.Phone7 = (UInt32)phone7;
                        pi.PhoneNum = phone;

                        return FindPhone(pi);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("not num {0}\n", e.ToString());
                    return new PhoneRecord { };

                }
            }

            return new PhoneRecord { };
        }

        /// <summary>
        /// 利用字典数组查找 phone.dat数据库中手机号码的信息
        /// </summary>
        /// <param name="pi"></param>
        /// <returns>返回查找到的手机号码信息</returns>
        private PhoneRecord FindPhone(PhoneInfo pi)
        {
            PhoneRecord pr = new PhoneRecord();
            string s;
            string[] strTemp;
            if (!dicBuf.ContainsKey((int)pi.Phone7))
            {
                pr.Set(pi.PhoneNum, "null", "null", "null", "null", "null");
                return pr;
            }
            s = dicBuf[(int)pi.Phone7];
            strTemp = s.Split(new char[] { '|' });
            pr.Set(pi.PhoneNum, strTemp[0], strTemp[1], strTemp[2], strTemp[3], GetCardType(Convert.ToByte(strTemp[4])));
            return pr;
        }

    }
}
