using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Phonedata
{
   public class Phonedata
    {
        private static  Int32 indexOffset; //第一个偏移量
        private static  Int32 total_len; //文件总长度
        private static  string Version = "";  //版本号
        private static FileStream content; // 读取的文件内容
        private static Int32 total_record; //记录数据量
        private static Int32 PHONE_INDEX_LENGTH = 9; //
        private static Int32 INT_LEN = 4;//
        private static byte[] Buf; //存储所有的数据于byte[]中
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
           override  public string ToString()
            {
                string tmp = string.Format("PhoneNum: {0}\nAreaZon: {1}\nCardType: {2}\nCity: {3}\nZipCode: {4}\nProvince: {5}\n",this.PhoneNum,this.AreaZon,this.CardType,this.City,this.ZipCode,this.Province);
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
        public Phonedata()
        {
            string phone_dat_path = "phone.dat";
            Init(phone_dat_path);
        }

        /// <summary>
        /// class phonedata 的构造函数
        /// </summary>
        public Phonedata(string phonedata)
        {
            Init(phonedata);
        }

        /// <summary>
        /// class phonedata 的析构函数
        /// </summary>
        ~Phonedata()
        {
            content.Close();
        }
        /// <summary>  
        /// 截取字节数组  
        /// </summary>  
        /// <param name="srcBytes">要截取的字节数组</param>  
        /// <param name="startIndex">开始截取位置的索引</param>  
        /// <param name="length">要截取的字节长度</param>  
        /// <returns>截取后的字节数组</returns>  
        private  byte[] SubByte(byte[] srcBytes, int startIndex, int length)
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

        //将Stream 转换成 byte[]
        /// <summary>
        /// 将 Stream类型转换成 byte[]类型
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public  byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;

        }

        //turn 4 byte to int32
        /// <summary>
        /// 将 4个byte转换成 int32类型
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private  Int32 Get4(byte[] b)
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
        /// 报告指定的 System.Byte[] 在此实例中的第一个匹配项的索引。  
        /// </summary>  
        /// <param name="srcBytes">被执行查找的 System.Byte[]。</param>  
        /// <param name="searchBytes">要查找的 System.Byte[]。</param>  
        /// <returns>如果找到该字节数组，则为 searchBytes 的索引位置；如果未找到该字节数组，则为 -1。如果 searchBytes 为 null 或者长度为0，则返回值为 -1。</returns>  
        private int IndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }
            for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { return i; }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { return i; }
                }
            }
            return -1;
        }

        /// <summary>
        ///  初始化程序
        /// </summary>
        public void Init(string phonedata)
        {
            byte[] tmp = new byte[4];

            if (!File.Exists(phonedata))
                {
                    return;
                }
            content = File.Open(phonedata, FileMode.Open);

            Buf = new byte[content.Length];
            Buf =   StreamToBytes(content);

            tmp = SubByte(Buf, 0, 4);
            Version = System.Text.Encoding.Default.GetString(tmp);
           
            total_len = (int)content.Length;

            indexOffset = Get4(SubByte(Buf, 4, 4));

            total_record = (total_len - indexOffset) / 9;

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
        public  PhoneRecord  Lookup(Int64 phone)
        {
            PhoneInfo pi = new PhoneInfo { };
            pi.PhoneNum = phone.ToString();
            if (phone >= 1000000 && phone <= 99999999999)
            {
                while (phone > 9999999)
                {
                    phone /= 10;
                }
                pi.Phone7 =(UInt32) phone;

                return FindPhone(pi);
            }
            return  new PhoneRecord{ };
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
        /// 利用二分法查找 phone.dat数据库中手机号码的信息
        /// </summary>
        /// <param name="pi"></param>
        /// <returns>返回查找到的手机号码信息</returns>
        private  PhoneRecord FindPhone(PhoneInfo pi)
        {
            PhoneRecord pr = new PhoneRecord { };
            Int32 phone_seven_int32;
            int left = 0;
            int right = 0;
            int total_len = Phonedata.total_len;
            int mid = 0;
            int offset = 0;
            Int32 cur_phone, record_offset;
            byte card_type;
            string card_str;
            string data_str;
            string[] data_str_arr;
            Int32 end_offset = 0;
            byte[] Zero_end = System.Text.Encoding.Default.GetBytes("\0");
            byte[] data;
            
            phone_seven_int32 = (Int32)pi.Phone7; //将uint32 转换为int32, 好用于对比
            right = (total_len - indexOffset) / PHONE_INDEX_LENGTH;

            for (; ; ) {
                if (left > right)
                {
                    return pr;
                }
                //mid = (left + right) / 2;
                mid = (left + right) >> 1;
                offset = indexOffset + mid * PHONE_INDEX_LENGTH;
                if (offset >= total_len)
                {
                    Console.WriteLine("offset >= total_len");
                    return pr;
                }
                cur_phone = Get4(SubByte(Buf, offset, INT_LEN));
                record_offset = Get4(SubByte(Buf, offset + INT_LEN, 4));

                card_type = SubByte(Buf, offset + INT_LEN * 2, 1)[0];

                if (cur_phone > phone_seven_int32)
                {
                    right = mid - 1;
                } else if (cur_phone < phone_seven_int32)
                {
                    left = mid + 1;
                } else
                {
                    byte[] cbyte = SubByte(Buf, record_offset,total_len - record_offset);
                    end_offset = IndexOf(cbyte, Zero_end);
                    data = SubByte(Buf, record_offset, end_offset);

                    data_str = System.Text.Encoding.UTF8.GetString(data);

                    data_str_arr = data_str.Split('|');
                    card_str = GetCardType(card_type);
                    pr.Set(pi.PhoneNum, data_str_arr[0], data_str_arr[1], data_str_arr[2], data_str_arr[3], card_str);

                    return pr;
                }

            }
            return pr;
        }

    }
}
