using System.IO;
using UnityEngine;

public class TGAFile {

    public struct tga_info {
        public char IDlength;
        public char ColorMapType;
        public char ImageType;
        public System.Int16 ColorMapStart;
        public System.Int16 ColorMapLength;
        public char ColorMapBits;
        public System.Int16 XOrigin;
        public System.Int16 YOrigin;
        public System.Int16 Width;
        public System.Int16 Height;
        public char Depth;
        public char Bits;
    };

    public static void SaveTGA(string filename, Texture2D texture)
    {
        if (Path.GetExtension(filename).Length == 0)
            filename += ".tga";

        tga_info info = new tga_info();
        info.IDlength = (char)0;
        info.ColorMapType = (char)0;
        info.ImageType = (char)2;
        info.ColorMapStart = (short)0;
        info.ColorMapLength = (short)0;
        info.ColorMapBits = (char)0;
        info.XOrigin = (short)0;
        info.YOrigin = (short)0;
        info.Width = (short)texture.width;
        info.Height = (short)texture.height;

        switch(texture.format)
        {
            case TextureFormat.RGB24:
            case TextureFormat.DXT1:
                info.Depth = (char)24;
                info.Bits = (char)8;
                break;

            case TextureFormat.RGBA32:
            case TextureFormat.DXT5:
                info.Depth = (char)32;
                info.Bits = (char)8;
                break;

            default:
                Debug.Log("Texture format not supported for TGA export: " + texture.format);
                return;
        }

        using (BinaryWriter targaFile = new BinaryWriter(File.Open(filename, FileMode.Create)))
        {
            // File Start Header offset 0
            targaFile.Write(info.IDlength);
            targaFile.Write(info.ColorMapType);
            targaFile.Write(info.ImageType);

            // Color Map specification offset 3
            targaFile.Write(info.ColorMapStart);
            targaFile.Write(info.ColorMapLength);
            targaFile.Write(info.ColorMapBits);

            // Image specification offset 8
            targaFile.Write(info.XOrigin);
            targaFile.Write(info.YOrigin);
            targaFile.Write(info.Width);
            targaFile.Write(info.Height);
            targaFile.Write(info.Depth);
            targaFile.Write(info.Bits);

            // Image Data Field offset 18+
            byte[] data = texture.GetRawTextureData();
            Color c = new Color();
            //Color[] c = texture.GetPixels();
            switch ((int)info.Depth)
            {
                case 24:
                    for (int i = 0; i < info.Width; i++)
                        for (int j = 0; j < info.Height; j++)
                        {
                            c = texture.GetPixel(i, j) * 255;
                            targaFile.Write((byte)c.b);
                            targaFile.Write((byte)c.g);
                            targaFile.Write((byte)c.r);
                        }
                    break;
                case 32:
                    for (int i = 0; i < info.Width; i++)
                        for (int j = 0; j < info.Height; j++)
                        {
                            c = texture.GetPixel(j, i) * 255;
                            targaFile.Write((byte)c.b);
                            targaFile.Write((byte)c.g);
                            targaFile.Write((byte)c.r);
                            targaFile.Write((byte)c.a);
                        }
                    break;
                case 8:
                case 16:
                    break;
            }

            targaFile.Close();
        }
    }

    public static Texture2D LoadTGA(string filename)
    {
        Texture2D texture = null;
        tga_info info = new tga_info();

        if (Path.GetExtension(filename).Length == 0)
            filename += ".tga";

        if (File.Exists(filename))
        {
            using (BinaryReader targaFile = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                // File Start Header offset 0
                info.IDlength = targaFile.ReadChar();
                info.ColorMapType = targaFile.ReadChar();
                info.ImageType = targaFile.ReadChar();

                // Color Map specification offset 3
                info.ColorMapStart = targaFile.ReadInt16();
                info.ColorMapLength = targaFile.ReadInt16();
                info.ColorMapBits = targaFile.ReadChar();

                // Image specification offset 8
                info.XOrigin = targaFile.ReadInt16();
                info.YOrigin = targaFile.ReadInt16();
                info.Width = targaFile.ReadInt16();
                info.Height = targaFile.ReadInt16();
                info.Depth = targaFile.ReadChar();
                info.Bits = targaFile.ReadChar();

                TextureFormat tf; 
                if (info.Depth == (char)24 && info.Bits == (char)8)
                    tf = TextureFormat.RGB24;
                else if (info.Depth == (char)32 && info.Bits == (char)8)
                    tf = TextureFormat.RGBA32;
                else
                {
                    Debug.Log("Texture format not supported for TGA import");
                    return null;
                }

                texture = new Texture2D(info.Width, info.Height, tf, false);
                int nbPixels = info.Width * info.Height;
                Color[] c = new Color[nbPixels];
                switch ((int)info.Depth)
                {
                    case 24:
                        for (int i = 0; i < nbPixels; ++i)
                        {
                            c[i] = new Color();
                            c[i].b = targaFile.ReadByte() / 255.0f;
                            c[i].g = targaFile.ReadByte() / 255.0f;
                            c[i].r = targaFile.ReadByte() / 255.0f;
                        }
                        break;
                    case 32:
                        for (int i = 0; i < nbPixels; ++i)
                        {
                            c[i] = new Color();
                            c[i].b = targaFile.ReadByte() / 255.0f;
                            c[i].g = targaFile.ReadByte() / 255.0f;
                            c[i].r = targaFile.ReadByte() / 255.0f;
                            c[i].a = targaFile.ReadByte() / 255.0f;
                        }
                        break;
                    case 8:
                    case 16:
                        break;
                }
                texture.SetPixels(c);

                targaFile.Close();
            }
        }

        return texture;
    }
}
