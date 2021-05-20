using System;
using System.Collections.Generic;
using System.Linq;
using TinyJSON;

namespace Spaces.Core
{
    /// <summary>
    /// Summary description for LoadJson
    /// </summary>
    public static class JSONTools
    {
        /// <summary>
        /// Loads a JSON file from disk and deserializes its contents into an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns>The deserialized JSON object</returns>
        public static T Load<T>(string path)
        {
            T returnValue;

            if (path != "")
            {
                string readFile;

                using (System.IO.StreamReader file = new System.IO.StreamReader(path))
                {
                    readFile = file.ReadToEnd();
                }

                TinyJSON.Variant variant = TinyJSON.Decoder.Decode(readFile);
                variant.Make<T>(out returnValue);

                return returnValue;
            }

            return default(T);
        }

        /// <summary>
        /// Deserializes a JSON string into an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns>The deserialized JSON object</returns>
        public static T LoadFromString<T>(string json)
        {
            T returnValue;

            if (json != "")
            {
                TinyJSON.Variant variant = TinyJSON.Decoder.Decode(json);
                variant.Make<T>(out returnValue);

                return returnValue;
            }

            return default(T);
        }

        /// <summary>
        /// Loads a JSON file from disk and place its contents into a string
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A string value representing the contents of the JSON file</returns>
        public static string LoadToString(string path)
        {
            string returnFile = "";

            if (path != "")
            {


                using (System.IO.StreamReader file = new System.IO.StreamReader(path))
                {
                    returnFile = file.ReadToEnd();
                }
            }

            return returnFile;
        }

        public static string LoadToString(object obj)
        {
            return TinyJSON.Encoder.Encode(obj);
        }

        public static DateTime ParseDateTimeFromString(string dateTime)
        {
            DateTime outputDateTime = new DateTime();
            string truncatedDateTime = dateTime.Length > 26 ? dateTime.Substring(0, 26) : dateTime.TrimEnd("Z".ToCharArray());

            DateTime.TryParseExact(truncatedDateTime + "Z", "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z", null, System.Globalization.DateTimeStyles.AssumeUniversal, out outputDateTime);

            return outputDateTime;
        }
    }
}