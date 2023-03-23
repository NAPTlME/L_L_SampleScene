using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

public class To3DFrom2DArrayTexture : MonoBehaviour
{
    [MenuItem("TextureArray/3D Texture From 2D Array (RGB)")]
    static void FromRgb()
    {
        Create3DTextureFrom2DArray(false);
    }
    [MenuItem("TextureArray/3D Texture From 2D Array (RGBA)")]
    static void FromRgba()
    {
        Create3DTextureFrom2DArray(true);
    }
    static void Create3DTextureFrom2DArray(bool isRgba)
    {
        // get input directory
        var inputDir = EditorUtility.OpenFolderPanel("Input Directory", "", "");
        // get output file
        var outputFile = System.IO.Path.GetRelativePath("./", EditorUtility.SaveFilePanel("Output Asset", "Assets/", "", "asset"));
        
        // read
        Debug.Log(inputDir.ToString());
        Debug.Log(outputFile.ToString());
        string[] allowedExtensions = new string[] { ".png", ".exr" };
        var inputPaths = Directory.GetFiles(inputDir).
            Where(wh =>
            { Debug.Log(System.IO.Path.GetExtension(wh));
                return allowedExtensions.Contains(System.IO.Path.GetExtension(wh));
                }).ToArray();

        if (inputPaths.Length == 0) {
            throw new System.Exception("No files found in input directory of type `.png` or `.exr`.");
        }
        //validate all have same extension
        var inputExtensions = inputPaths.Select(sel => System.IO.Path.GetExtension(sel)).ToArray();
        if (inputExtensions.Distinct().Count() != 1)
        {
            throw new System.Exception("Inconsistent file formats found in path.");
        }
        bool isPng = inputExtensions.First() == ".png"; // if not can assume exr

        TextureFormat format3d = isPng ? TextureFormat.RGB48 : TextureFormat.RGBA64;

        Texture3D outputTexture = new Texture3D(256, 256, inputPaths.Length, format3d, false);

        // convert to 3d
        Color[] pixelValues = Enumerable.Repeat<Color>(new Color(0, 0, 0, 0), 256 * 256 * inputPaths.Length).ToArray();

        //validate (all are of the same dimension) (validate in loop)
        for (int i = 0; i < inputPaths.Length; i++)
        {
            string path = inputPaths[i];
            Texture2D img = isPng ? ReadPng(path, isRgba) : LoadExr(path);
            var imgPixels = img.GetPixels();
            if (imgPixels.Length != 256 * 256)
            {
                throw new System.Exception("Incorrect number of pixels in " + path + ". Expected " + 256 * 256 + " found " + imgPixels.Length);
            }
            // load pixels into 3d array
            int startingIndex = i * 256 * 256;
            for (int j = 0; j < imgPixels.Length; j++)
            {
                pixelValues[startingIndex + j] = imgPixels[j];
            }
        }
        outputTexture.SetPixels(pixelValues);
        outputTexture.Apply();

        AssetDatabase.CreateAsset(outputTexture, outputFile);
        
    }
    static Texture2D ReadPng(string path, bool isRGBA)
    {
        Debug.Log("Reading Png file from: " + path);

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D thisTexture = new Texture2D(256, 256, isRGBA ? TextureFormat.RGBA64 : TextureFormat.RGB48, false);
        thisTexture.LoadImage(bytes);
        return thisTexture;
    }
    static Texture2D LoadExr(string path)
    {
        // doesn't currently work
        throw new System.Exception("Not implemented");
        string pathRelative = System.IO.Path.GetRelativePath("./", path);
        Debug.Log("Loading from: " + pathRelative);
        Texture2D returnTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pathRelative);
        Debug.Log("Pixel count: " + returnTexture.GetPixels().Length);
        return returnTexture;
    }
}
