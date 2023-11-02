using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public static class FileReadAndWrite
{

    //读filename到byte[]

    static public byte[] ReadFile(string fileName)

    {

        FileStream pFileStream = null;

        byte[] pReadByte = new byte[0];

        try

        {

            pFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            BinaryReader r = new BinaryReader(pFileStream);

            r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开

            pReadByte = r.ReadBytes((int)r.BaseStream.Length);

            return pReadByte;

        }

        catch

        {

            return pReadByte;

        }

        finally

        {

            if (pFileStream != null)

                pFileStream.Close();

        }

    }

    //写byte[]到fileName

    static public bool WriteFile(byte[] pReadByte, string fileName)

    {

        FileStream pFileStream = null;



        try

        {

            pFileStream = new FileStream(fileName, FileMode.OpenOrCreate);

            pFileStream.Write(pReadByte, 0, pReadByte.Length);



        }

        catch

        {

            return false;

        }

        finally

        {

            if (pFileStream != null)

                pFileStream.Close();

        }

        return true;

    }




    static public string ReadFileByString(string filename)
    {
        string readFileBuffer = "";
        StreamReader streamReader = new StreamReader(filename, Encoding.Default);
        readFileBuffer = streamReader.ReadToEnd();
        streamReader.Close();
        return readFileBuffer;
    }

    static public string ReadFileByString(string filename, Encoding encoding)
    {
        string readFileBuffer = "";
        StreamReader streamReader = new StreamReader(filename, encoding);
        readFileBuffer = streamReader.ReadToEnd();
        streamReader.Close();
        return readFileBuffer;
    }

    static public string WriteFileByString(string outputFileName, string inputString)
    {
        string err = "";
        try
        {
            byte[] data = Encoding.GetEncoding("GB2312").GetBytes(inputString);

            FileStream fs = new FileStream(outputFileName, FileMode.Create);

            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        catch (Exception e)
        {
            err = e.ToString();
        }
        return err;
    }





}
