using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoowGood.Core.Service
{
    public class StandardDataAccessMessages
    {
        public static string SuccessMessaage = "Saved Successfully";
        public static string ErrorOnAddMessaage = "Error on adding";
        public static string DeleteMessaage(string record)
        {
            return $"The {record} was deleted.";
        }

        public static string GetSqlErrorMessage(Exception ex, string fieldName = "Record")
        {
            if (ex.Message == null)
            {
                return "";
            }

            if (ex.Message.Contains("insert duplicate key"))
            {
                return $"{fieldName} already exist.";
            }
            else if (ex.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
            {
                return $"{fieldName} already in used.";
            }
            else if (ex.Message.Contains("Login failed for user"))
            {
                return $"Invalid connection setup. Please contact with administrator.";
            }

            return ex.Message.Substring(0, Math.Min(500, ex.Message.Length));
        }
    }
}
