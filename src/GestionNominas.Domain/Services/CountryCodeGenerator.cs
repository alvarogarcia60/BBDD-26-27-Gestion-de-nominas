using System;
using System.Globalization;
using System.Text;
using GestionNominas.Domain.Entities;

namespace GestionNominas.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio: Generador de códigos identificadores de países.
    /// Implementa el algoritmo requerido en el laboratorio:
    /// 1. Toma las 3 primeras letras del nombre en mayúsculas (ej. ESP para España).
    /// 2. Si ya existe en la BD por clave primaria, sustituye la tercera letra por la siguiente letra posible del nombre.
    ///    Ejemplo: si ya existe Australia (AUS), al insertar Austria toma AUT.
    /// </summary>
    public static class CountryCodeGenerator
    {
        public static string GenerateCode(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                throw new ArgumentException("El nombre del país no puede estar vacío.");
            }

            // Eliminar diacríticos (acentos) y caracteres no alfabéticos
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
                // Rellenar con 'X' si el nombre fuera extremadamente corto (ej. "Do")
                lettersStr = lettersStr.PadRight(3, 'X');
            }

            // 1. Probar el código estándar con las 3 primeras letras (índices 0, 1, 2)
            string firstTwo = lettersStr.Substring(0, 2);
            string baseCode = lettersStr.Substring(0, 3);

            if (!Country.Exists(baseCode))
            {
                return baseCode;
            }

            // 2. Si ya existe, probar las siguientes letras disponibles del nombre a partir del índice 3
            for (int i = 3; i < lettersStr.Length; i++)
            {
                string candidate = firstTwo + lettersStr[i];
                if (!Country.Exists(candidate))
                {
                    return candidate;
                }
            }

            // 3. Si todas las letras del nombre colisionan, probar el abecedario de 'A' a 'Z'
            for (char c = 'A'; c <= 'Z'; c++)
            {
                string candidate = firstTwo + c;
                if (!Country.Exists(candidate))
                {
                    return candidate;
                }
            }

            // Fallback genérico
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
