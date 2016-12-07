using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Utils
{
    private static string accountName;

    public static string GetAccountName()
    {
        return accountName;
    }

    public static void SetAccountName(string account)
    {
        accountName = account;
    }

}
