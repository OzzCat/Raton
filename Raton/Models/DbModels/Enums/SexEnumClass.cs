using System;

namespace Raton.Models.DbModels.Enums
{
    public static class SexEnumClass
    {
        public enum SexEnum
        {
            F = 0, 
            M = 1, 
            NS = 2
        }

        public static Func<string, SexEnum> ConvertStringToSexEnum = str =>
        {
            switch (str)
            {
                case "F":
                    return SexEnum.F;
                case "f":
                    return SexEnum.F;
                case "M":
                    return SexEnum.M;
                case "m":
                    return SexEnum.M;
                default:
                    return SexEnum.NS;
            }
        };

        

        public static Func<SexEnum, string> ConvertFromSexEnumToString = en =>
        {
            switch (en)
            {
                case SexEnum.NS:
                    return "NS";
                case SexEnum.M:
                    return "M";
                case SexEnum.F:
                    return "F";
                default:
                    throw new IndexOutOfRangeException();
            }
        };
    }
}
