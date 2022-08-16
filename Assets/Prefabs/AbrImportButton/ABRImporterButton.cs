using UnityEngine;
using UnityEngine.UI;
using System.IO;
using MiscUtil.Conversion;
using MiscUtil.IO;
using SimpleFileBrowser;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class ABRImporterButton : MonoBehaviour, IPointerDownHandler
{
    private List<Texture2D> brushes = new List<Texture2D>();
    public GameObject DEBUG1, DEBUG2, DEBUG3;

    // The input raw image is matches the size of parent while maintaining ratio of the texture
    public Vector2 SizeToParent(RawImage image, float padding = 0)
    {
        var parent = image.transform.parent.GetComponentInParent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
        if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
        padding = 1 - padding;
        float w = 0, h = 0;
        float ratio = image.texture.width / (float)image.texture.height;
        var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
        if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
        {
            //Invert the bounds if the image is rotated
            bounds.size = new Vector2(bounds.height, bounds.width);
        }
        //Size by height first
        h = bounds.height * padding;
        w = h * ratio;
        if (w > bounds.width * padding)
        { //If it doesn't fit, fallback to width;
            w = bounds.width * padding;
            h = w / ratio;
        }
        imageTransform.sizeDelta = new Vector2(w, h);
        return imageTransform.sizeDelta;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Brushes", ".abr"));
        FileBrowser.ShowLoadDialog((paths) => { Debug.Log("Selected: " + paths[0]); },
                                   () => { Debug.Log("Canceled"); },
                                   FileBrowser.PickMode.Files, false, null, null, "Select Folder", "Select"); // These default settings will be overwritten in the coroutine

        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load File", "Load");

        Debug.Log("File Selection status: " + FileBrowser.Success);
        if (!FileBrowser.Success)
        {
            yield break;
        }

        // Results of selection is in FileBrowser.Result[]
        for (int i = 0; i < FileBrowser.Result.Length; i++)
        {
            Debug.Log(FileBrowser.Result[i]);
            ImportAsset(FileBrowser.Result[i]);
        }
    }

    private void Update()
    {
        if (brushes.Count > 3)
        {
            Debug.Log("HERE");
            Debug.Log(brushes.Count);
            DEBUG1.GetComponent<RawImage>().texture = brushes[0];
            DEBUG2.GetComponent<RawImage>().texture = brushes[1];
            DEBUG3.GetComponent<RawImage>().texture = brushes[2];

            SizeToParent(DEBUG1.GetComponent<RawImage>());
            SizeToParent(DEBUG2.GetComponent<RawImage>());
            SizeToParent(DEBUG3.GetComponent<RawImage>());
        }
    }

    public void ImportAsset(string path)
    {
        var converter = new BigEndianBitConverter();
        using (var fs = File.Open(path, FileMode.Open))
        {
            var ebr = new EndianBinaryReader(converter, fs);

            int ver = ebr.ReadInt16();
            switch (ver)
            {
                case 1:
                    ReadVer12(ebr, ver, path);
                    break;

                case 2:
                    ReadVer12(ebr, ver, path);
                    break;

                case 6:
                    ReadVer6(ebr, path);
                    break;

                default:
                    Debug.LogError("Unsupported file version");
                    break;
            }
        }
    }

    private void ReadVer6(EndianBinaryReader ebr, string path)
    {
        int width = 0;
        int height = 0;
        int num3 = ebr.ReadInt16();
        ebr.ReadBytes(8);
        int num5 = ebr.ReadInt32() + 12;
        int index = 0;
        while (ebr.BaseStream.Position < (num5 - 1))
        {
            int num6 = ebr.ReadInt32();
            int num7 = num6;
            while ((num7 % 4) != 0)
                num7++;

            int num8 = num7 - num6;
            ebr.ReadString();

            switch (num3)
            {
                case 1:
                    ebr.ReadInt16();
                    ebr.ReadInt16();
                    ebr.ReadInt16();
                    ebr.ReadInt16();
                    ebr.ReadInt16();
                    int num9 = ebr.ReadInt32();
                    int num10 = ebr.ReadInt32();
                    int num11 = ebr.ReadInt32();
                    width = ebr.ReadInt32() - num10;
                    height = num11 - num9;
                    break;

                case 2:
                    ebr.ReadBytes(0x108);
                    int num13 = ebr.ReadInt32();
                    int num14 = ebr.ReadInt32();
                    int num15 = ebr.ReadInt32();
                    width = ebr.ReadInt32() - num14;
                    height = num15 - num13;
                    break;
            }

            ebr.ReadInt16();

            byte[] buffer;
            if (ebr.ReadByte() == 0)
            {
                buffer = ebr.ReadBytes(width * height);
            }
            else
            {
                int num18 = 0;
                for (int j = 0; j < height; j++)
                    num18 += ebr.ReadInt16();

                byte[] imgdata = ebr.ReadBytes(num18);
                buffer = Unpack(imgdata);
            }

            Texture2D tex = CreateImage(width, height, buffer);
            tex.alphaIsTransparency = true;
            string name = $"{System.IO.Path.GetFileNameWithoutExtension(path)}_{index}";
            tex.name = name;
            brushes.Add(tex);

            index++;

            switch (num3)
            {
                case 1:
                    ebr.ReadBytes(num8);
                    continue;
                case 2:
                    ebr.ReadBytes(8);
                    ebr.ReadBytes(num8);
                    break;
            }
        }
    }

    private void ReadVer12(EndianBinaryReader ebr, int ver, string path)
    {
        int num = ebr.ReadInt16();
        for (int i = 0; i < num; i++)
        {
            int num3 = ebr.ReadInt16();
            int num4 = ebr.ReadInt32();
            switch (num3)
            {
                case 1:
                    {
                        if (ver == 1)
                        {
                            ebr.ReadBytes(14);
                        }
                        if (ver == 2)
                        {
                            ebr.ReadBytes(num4);
                        }
                        break;
                    }
                case 2:
                    {
                        ebr.ReadInt32();
                        ebr.ReadInt16();
                        if (ver == 1)
                        {
                            ebr.ReadByte();
                        }
                        if (ver == 2)
                        {
                            int num5 = ebr.ReadInt32();
                            ebr.ReadBytes(num5 * 2);
                            ebr.ReadBytes(1);
                        }
                        ebr.ReadInt16();
                        ebr.ReadInt16();
                        ebr.ReadInt16();
                        ebr.ReadInt16();
                        int num6 = ebr.ReadInt32();
                        int num7 = ebr.ReadInt32();
                        int num8 = ebr.ReadInt32();
                        int num9 = ebr.ReadInt32();
                        ebr.ReadInt16();
                        int num10 = ebr.ReadByte();
                        int width = num9 - num7;
                        int height = num8 - num6;

                        byte[] buffer;
                        if (num10 == 0)
                        {
                            buffer = ebr.ReadBytes(width * height);
                        }
                        else
                        {
                            int num13 = 0;
                            for (int k = 0; k < height; k++)
                            {
                                num13 += ebr.ReadInt16();
                            }

                            byte[] imgdata = ebr.ReadBytes(num13);
                            buffer = Unpack(imgdata);
                        }

                        Texture2D tex = CreateImage(width, height, buffer);
                        tex.alphaIsTransparency = true;
                        string name = $"{System.IO.Path.GetFileNameWithoutExtension(path)}_{num}";
                        tex.name = name;
                        brushes.Add(tex);
                        break;
                    }
            }
        }
    }

    private Texture2D CreateImage(int width, int height, byte[] buffer)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.Alpha8, false);

        int pixelIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, new Color32(0, 0, 0, buffer[pixelIndex++]));
            }
        }

        tex.Apply();
        return tex;
    }

    private byte[] Unpack(byte[] imgdata)
    {
        using (var input = new MemoryStream(imgdata))
        using (var output = new MemoryStream(imgdata.Length))
        {
            var reader = new BinaryReader(input);
            var writer = new BinaryWriter(output);

            var length = imgdata.Length - sizeof(byte);
            while (input.Position < length)
            {
                sbyte count = reader.ReadSByte();
                if (count >= 0)
                {
                    writer.Write(reader.ReadBytes(count + 1));
                }
                else
                {
                    byte value = reader.ReadByte();
                    while (count++ <= 0)
                        writer.Write(value);
                }
            }
            return output.ToArray();
        }
    }
}