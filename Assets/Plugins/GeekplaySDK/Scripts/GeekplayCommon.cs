using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

using Com.Itrus.Crypto;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Encoders;

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

    public static byte[] HexStringToBytes(string _str)
    {
        int length = _str.Length / 2;
        byte[] result = new byte[length];

        for (int i = 0; i < length; ++i)
        {
            string temp = _str.Substring(2 * i, 2);
            result[i] = (byte)Convert.ToInt32(temp, 16);
        }

        return result;
    }

    public static bool VerifySign(string _dataToSign, string _signPack, string _pubKeyX, string _pubKeyY)
    {
        SM2 sm2 = SM2.Instance;
        sm2.cipher_sm = new SM2.Cipher();

        string signPack1 = _signPack.Substring(0, 64);
        string signPack2 = _signPack.Substring(64, 64);

        BigInteger ecc_gx = new BigInteger(_pubKeyX, 16);
        BigInteger ecc_gy = new BigInteger(_pubKeyY, 16);
        
        ECFieldElement ecc_gx_fieldElement = sm2.ecc_curve.FromBigInteger(ecc_gx); //选定椭圆曲线上基点 G 的 x 坐标
        ECFieldElement ecc_gy_fieldElement = sm2.ecc_curve.FromBigInteger(ecc_gy); //选定椭圆曲线上基点 G 的 y 坐标
        ECPoint ecc_point_g = new FpPoint(sm2.ecc_curve, ecc_gx_fieldElement, ecc_gy_fieldElement); //生成基点 G

        SM2.SM2Result result = new SM2.SM2Result();

        BigInteger gp_br = new BigInteger(signPack1, 16);
        BigInteger gp_bs = new BigInteger(signPack2, 16);
        result.r = gp_br;
        result.s = gp_bs;
        
        sm2.Sm2Verify(HexStringToBytes(_dataToSign), ecc_point_g, gp_br, gp_bs, result);

        return result.R.Equals(result.r);
    }
}
