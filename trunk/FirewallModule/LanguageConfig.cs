using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace FM
{
    public static class LanguageConfig
    {
        public enum Language
        {
            ENGLISH,
            SPANISH,
            GERMAN,
            CHINESE,
            RUSSIAN,
            NONE
        }

        static Language cLanguage = Language.NONE;

        public static Language GetCurrentLanguage()
        {
            if (cLanguage == Language.NONE)
            {
                switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                {
                    case "en":
                        cLanguage = Language.ENGLISH;
                        break;
                    case "es":
                        cLanguage = Language.SPANISH;
                        break;
                    case "de":
                        cLanguage = Language.GERMAN;
                        break;
                    case "zh":
                        cLanguage = Language.CHINESE;
                        break;
                    case "ru":
                        cLanguage = Language.RUSSIAN;
                        break;
                }
            }
            return cLanguage;
        }

    }
}
