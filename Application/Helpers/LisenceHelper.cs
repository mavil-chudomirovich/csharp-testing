using Application.AppExceptions;
using Application.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class LisenceHelper
    {
        public static int MapLicenseClass(string cls)
        {
            return cls.ToUpper() switch
            {
                "B1" => 0,
                "B" => 1,
                "C1" => 2,
                "C" => 3,
                "D1" => 4,
                "D2" => 5,
                "D" => 6,
                "BE" => 7,
                "C1E" => 8,
                "CE" => 9,
                "D1E" => 10,
                "D2E" => 11,
                "DE" => 12,
                _ => -1 // unknown
            };
        }
        public static void EnsureMatch(string nameA, DateTimeOffset dobA,
                                   string nameB, DateTimeOffset dobB)
        {
            // normalize
            var nA = Normalize(nameA);
            var nB = Normalize(nameB);

            if (!string.Equals(nA, nB, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException(Message.UserMessage.InvalidNameOnOtherPaper);

            if (dobA.Date != dobB.Date)
                throw new BusinessException(Message.UserMessage.InvalidDateOnOtherPaper);
        }

        private static string Normalize(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "";
            var parts = fullName.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            return string.Join(' ', parts);
        }
        public static void ValidateCitizenIdNumber(string number)
        {
            number = number.Trim();

            if (!number.All(char.IsDigit))
                throw new BadRequestException(Message.UserMessage.InvalidCitizenIdFormat);

            if (number.Length != 12 && number.Length != 9)
                throw new BadRequestException(Message.UserMessage.InvalidCitizenIdFormat);
        }

        public static void ValidateDriverLicenseNumber(string number)
        {
            number = number.Trim();

            if (!number.All(char.IsDigit))
                throw new BadRequestException(Message.UserMessage.InvalidDriverLicenseFormat);

            if (number.Length < 10 || number.Length > 12)
                throw new BadRequestException(Message.UserMessage.InvalidDriverLicenseFormat);
        }
    }
}
