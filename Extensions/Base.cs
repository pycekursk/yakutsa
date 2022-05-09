using Microsoft.AspNetCore.Html;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using yakutsa;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RetailCRMCore.Models;

namespace yakutsa.Extensions
{


    public static class ImageResizer
    {

        public enum ImageType
        {
            png = 0,
            jpg = 1,
            webp = 2,
            jpeg = 3
        }

        public static async Task<string> Resize(string inputPath, ImageType imageType, string? outputPath = null, int width = 0, int height = 0, int quality = 100)
        {
            int size = width == 0 ? height : width;
            HttpClient client = new HttpClient();
            var byteArray = await client.GetByteArrayAsync(inputPath);
            using (var ms = new MemoryStream())
            {
                await ms.WriteAsync(byteArray, 0, byteArray.Length);

                using (var image = new Bitmap(ms))
                {
                    if (image.Width > image.Height)
                    {
                        width = size;
                        height = Convert.ToInt32(image.Height * size / (double)image.Width);
                    }
                    else
                    {
                        width = Convert.ToInt32(image.Width * size / (double)image.Height);
                        height = size;
                    }

                    var resized = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.DrawImage(image, 0, 0, width, height);

                        var qualityParamId = Encoder.Quality;
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);

                        ImageFormat imageFormat = ImageFormat.Png;
                        switch (imageType)
                        {
                            case ImageType.png:
                                imageFormat = ImageFormat.Png;
                                break;
                            case ImageType.jpg:
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            case ImageType.jpeg:
                                imageFormat = ImageFormat.Jpeg;
                                break;
                            default: break;
                        }
                        ImageCodecInfo? codec = ImageCodecInfo.GetImageDecoders()
                            .FirstOrDefault(codec => codec.FormatID == imageFormat.Guid);

                        if (outputPath != null) resized.Save(outputPath, codec, encoderParameters);

                        using (var ms2 = new MemoryStream())
                        {
                            resized.Save(ms2, codec, encoderParameters);
                            return $"data:image/{imageType};base64,{Convert.ToBase64String(ms2.ToArray())}";
                        }
                    }
                }
            }
        }
    }
    static public class Base
    {




        public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue)
          where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        public static string? GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetAttribute<DisplayAttribute>()?.Name?.ToString();
        }

        public static string? GetDisplayName(this PropertyInfo propertyInfo)
        {
            var displayName = propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name;
            return string.IsNullOrEmpty(displayName) ? propertyInfo.Name : displayName;
        }

        public static string? GetPropertyAction(this object obj)
        {
            var displayName = obj.GetType().GetCustomAttribute<ActionAttribute>()?.ActionName;
            return string.IsNullOrEmpty(displayName) ? obj.ToString() : displayName;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return obj.GetType().GetProperties()
                 .Single(pi => pi.Name == propertyName)
                 .GetValue(obj, null);
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperties()
                 .Single(pi => pi.Name == propertyName)
                 .SetValue(obj, value);
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return obj.GetType().GetFields()
                 .Single(pi => pi.Name == fieldName)
                 .GetValue(obj);
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetProperties()
                 .Single(pi => pi.Name == fieldName)
                 .SetValue(obj, value);
        }

    

        public static List<string> ToArray<T>()
        {
            List<string> vs = new List<string>();
            var arr = Enum.GetValues(typeof(T));
            foreach (var item in arr)
            {
                vs.Add(item.ToString());
            }
            return vs;
        }

        public static string GetString(this IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
