using System.IO;
using UnityEngine;

namespace EUtil
{
    public static class SpriteUtil
    {
        // Credit goes to 0Mayall (see https://github.com/0Mayall/ONIMods/blob/master/Blueprints/Utilities.cs)
        public static Sprite CreateSpriteDXT5(Stream inputStream, int width, int height)
        {
            byte[] buffer = new byte[inputStream.Length - 128];
            inputStream.Seek(128, SeekOrigin.Current);
            inputStream.Read(buffer, 0, buffer.Length);

            Texture2D texture = new Texture2D(width, height, TextureFormat.DXT5, false);
            texture.LoadRawTextureData(buffer);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5F, 0.5F));
        }
    }
}
