using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace TestWebURL
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        StreamReader sr = new StreamReader(Console.OpenStandardInput());
        while (!sr.EndOfStream)
          CheckURL(sr.ReadLine());
      }
      else
      {
        string s = args[0].ToLower().Trim("-/".ToCharArray());
        if(s == "help" || s == "h" || s == "?")
        {
          Console.WriteLine("TestWebUrl - jorgie@missouri.edu - 2015");
          Console.WriteLine("  - Preforms some simples tests on URLs");
          Console.WriteLine("  - Pass one or more URLs on the commandline or via STDIN.");
          Console.WriteLine();
          return;
        }

        for (int x = 0; x < args.Length; x++)
          CheckURL(args[x]);
      }
    }

    private static void CheckURL(string URL)
    {
      URL = URL.Trim();
      if (URL == "") return;
      if (!URL.ToLower().StartsWith("http")) URL = "http://" + URL;

      Console.Write(URL);

      Uri uri = null;
      try { uri = new Uri(URL); }
      catch { }

      if (uri == null)
        Console.Write(", BAD_URI");
      else
      {
        IPAddress[] ips = null;

        try { ips = Dns.GetHostAddresses(uri.DnsSafeHost); }
        catch { }

        if (ips == null)
        {
          Console.Write(", DNS_FAIL");
        }
        else
        {
          Console.Write(", " + DoHead(uri));
          Console.Write(", " + DoGet(uri));
        }
      }

      Console.WriteLine();
    }

    static string DoHead(Uri URI)
    {
      WebRequest wr = WebRequest.Create(URI);
      wr.Method = "HEAD";
      HttpWebResponse res = null;
      string msg = "";
      string lastMod = "";

      DateTime sTime = DateTime.Now;

      try { res = (HttpWebResponse)wr.GetResponse(); }
      catch (WebException we)
      {
        res = (HttpWebResponse)we.Response;
        msg = we.Status.ToString();
      }

      TimeSpan delta = DateTime.Now.Subtract(sTime);

      if (res != null)
      {
        msg = string.Format("{0} {1}", (int)res.StatusCode, res.StatusDescription);
        lastMod = res.LastModified.ToString("u").Replace(" ", "_").TrimEnd('Z');
      }
      return string.Format("HEAD:{0}/{1}/{2}ms", msg, lastMod, delta.TotalMilliseconds);

    }

    static string DoGet(Uri URI)
    {
      HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(URI);
      wr.Method = "GET";
      wr.AllowAutoRedirect = false;
      wr.AuthenticationLevel = System.Net.Security.AuthenticationLevel.None;

      HttpWebResponse res = null;

      DateTime sTime = DateTime.Now;

      try { res = (HttpWebResponse)wr.GetResponse(); }
      catch (WebException we) { res = (HttpWebResponse)we.Response; }

      TimeSpan delta = DateTime.Now.Subtract(sTime);

      string msg = "";
      long length = 0;
      string lastMod = "";

      if (res != null) {
        msg = string.Format("{0} {1}", (int)res.StatusCode, res.StatusDescription);
        length = res.ContentLength;
        if (res.StatusCode == HttpStatusCode.Moved ||
            res.StatusCode == HttpStatusCode.MovedPermanently ||
            res.StatusCode == HttpStatusCode.Found ||
            res.StatusCode == HttpStatusCode.Redirect ||
            res.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            res.StatusCode == HttpStatusCode.RedirectMethod ||
            res.StatusCode == HttpStatusCode.TemporaryRedirect
            )
          msg = string.Format("{0}/[{1}]", msg, res.GetResponseHeader("location"));
        lastMod = res.LastModified.ToString("u").Replace(" ", "_").TrimEnd('Z');
      }

      return string.Format("GET:{0}/{1}/{2}ms/{3}bytes", msg, lastMod, delta.TotalMilliseconds, length);

    }

  }
}
