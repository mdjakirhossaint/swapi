
using MsgPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace SoowGood.Core.Service.Utility
{
    public class SharedService
    {
        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int GetRandomNo(int min, int max)
        {
            Random rdm = new Random();
            return rdm.Next(min, max);
        }
        public static string GetDisplayName(Enum enumValue)
        {
            if (enumValue != null)
            {
                var temp = enumValue.GetType().GetMember(enumValue.ToString())
                               .First();
                if (temp.GetCustomAttribute<DisplayAttribute>() != null)
                    return temp.GetCustomAttribute<DisplayAttribute>().Name;
                else
                    return temp.Name;
            }
            return "";
        }

    }


}
