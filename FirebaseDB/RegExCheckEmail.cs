using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/*
 * Author: Mykola Dobrochynskyy, PTL
 * Version 1.1
 * Created date:  07.01.2008
 * Last upadte date: 26-08-2017
 * Description: Implements the static class, that could be used to check an E-Mail address.
 * -----------------------------------------------------------------------------
 * You are eligible to use this code for you own purpouse assuming that this
 * code and information in it is provided "AS IS" without warranty of any kind
 * expressed or implied.
 * ------------------------------------------------------------------------------
 */

namespace RegExCheckEmail
{
    // Tests an E-Mail address.
    public static class CheckEmail
    {
        // Regular expression, which is used to validate an E-Mail address.
        public const string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        // Checks whether the given Email-Parameter is a valid E-Mail address.
        // <param name="email">Parameter-string that contains an E-Mail address.</param>
        ///<returns>True, wenn Parameter-string is not null and contains a valid E-Mail address;
        // otherwise false.</returns>
        public static bool IsEmail(string email)
        {
            if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
            else return false;
        }
    }
}
