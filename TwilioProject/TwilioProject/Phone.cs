using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace TwilioProject
{
    static public class Phone
    {
        static Regex phone1 = new Regex(@"[+]1\d{10}");
        static Regex phone2 = new Regex(@"[+]1[(]\d{3}[)]\d{7}");
        static Regex phone3 = new Regex(@"[+]1\d{3}[-]\d{3}[-]\d{4}");
        static Regex phone4 = new Regex(@"[+]1[(]\d{3}[)]\d{3}[-]\d{4}");
        static Regex phone5 = new Regex(@"[+]1[(]\d{3}[)][-]\d{3}[-]\d{4}");



        public static string Parse(string phoneNumber)
        {
            if (phone1.IsMatch(phoneNumber))
            {
                string temp = "";
                for (int i = 2; i < phoneNumber.Length; i++)
                {
                    temp += phoneNumber[i];
                }
                return temp;
            }
            else if (phone2.IsMatch(phoneNumber))
            {
                string temp = "";
                for (int i = 2; i < phoneNumber.Length; i++)
                {
                    if (i != 3 || i != 7)
                    {
                        temp += phoneNumber[i];
                    }
                }
                return temp;
            }
            else if (phone3.IsMatch(phoneNumber))
            {
                string temp = "";
                for (int i = 2; i < phoneNumber.Length; i++)
                {
                    if (i != 6 || i != 10)
                    {
                        temp += phoneNumber[i];
                    }
                }
                return temp;
            }
            else if (phone4.IsMatch(phoneNumber))
            {
                string temp = "";
                for (int i = 2; i < phoneNumber.Length; i++)
                {
                    if (i != 3 || i != 7 || i != 11)
                    {
                        temp += phoneNumber[i];
                    }
                }
                return temp;
            }
            else if (phone5.IsMatch(phoneNumber))
            {
                string temp = "";
                for (int i = 2; i < phoneNumber.Length; i++)
                {
                    if (i != 3 || i != 7 || i != 8 || i != 12)
                    {
                        temp += phoneNumber[i];
                    }
                }
                return temp;
            }
            else
            {
                return "Invalid Input";
            }
        }
    }
}