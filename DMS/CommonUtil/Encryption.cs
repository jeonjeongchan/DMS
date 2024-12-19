using System;
namespace DMS.CommonUtil
{
	public class Encryption
    {
        public static string CreateRandomKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

