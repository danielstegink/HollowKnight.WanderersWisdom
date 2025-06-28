using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WanderersWisdom.Helpers
{
    public static class SpriteHelper
    {
        /// <summary>
        /// Gets the charm's sprite (icon) from the mod's embedded resources
        /// </summary>
        /// <returns></returns>
        public static Sprite Get(string spriteFileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream($"WanderersWisdom.Resources.{spriteFileName}.png"))
            {
                // Convert stream to bytes
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                // Create texture from bytes
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes, true);

                // Create sprite from texture
                return Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
        }
    }

}
