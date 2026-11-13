using System;
using System.Globalization;
using System.Text;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Genera el código de un país nuevo: las tres primeras letras del nombre en mayúsculas
    /// (ESP para España). Si ese código ya existe, la tercera letra pasa a ser la siguiente
    /// letra posible del nombre (si existe AUS, Austria será AUT).
    /// </summary>
    public static class CountryCodeGenerator
    {
        public static string GenerateCode(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                throw new ArgumentException("El nombre del país no puede estar vacío.");
            }

            // Solo letras, sin tildes y en mayúsculas
            string cleanName = RemoveDiacritics(countryName).ToUpper();
            var letters = new StringBuilder();

            foreach (char c in cleanName)
            {
                if (char.IsLetter(c))
                {
                    letters.Append(c);
                }
            }

            string lettersStr = letters.ToString();
            if (lettersStr.Length < 3)
            {
                lettersStr = lettersStr.PadRight(3, 'X');
            }

            // 1. Las tres primeras letras
            string firstTwo = lettersStr.Substring(0, 2);
            string baseCode = lettersStr.Substring(0, 3);

            if (!Country.Exists(baseCode))
            {
                return baseCode;
            }

            // 2. Si ya existe, la tercera letra pasa a ser la siguiente letra del nombre
            for (int i = 3; i < lettersStr.Length; i++)
            {
                string candidate = firstTwo + lettersStr[i];
                if (!Country.Exists(candidate))
                {
                    return candidate;
                }
            }

            // 3. Si todas existen, se prueba con el resto del abecedario
            for (char c = 'A'; c <= 'Z'; c++)
            {
                string candidate = firstTwo + c;
                if (!Country.Exists(candidate))
                {
                    return candidate;
                }
            }

            return baseCode;
        }

        private static string RemoveDiacritics(string text)
        {
            string normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
