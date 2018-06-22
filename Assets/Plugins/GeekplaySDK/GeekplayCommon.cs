using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class GeekplayCommon
{
    public static string BytesToHexString(byte[] _data, string _splitChar)
    {
        string hexString = string.Empty;
        if (_data != null)
        {
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < _data.Length; i++)
            {
                if (i != _data.Length - 1)
                {
                    strB.Append(_data[i].ToString("X2") + _splitChar);
                }
                else
                {
                    strB.Append(_data[i].ToString("X2"));
                }
            }
            hexString = strB.ToString();
        }
        return hexString;
    }
}
