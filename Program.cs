using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TestWebURL
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1 || args[0].Length < 3)
      {
        Console.WriteLine("You must provide a URL on the command line.");
        Environment.Exit(99);
      }

      string url = args[0];

      if (!url.ToLower().StartsWith("http")) url = "http://" + url;

      Console.WriteLine(CheckURL(url));


    }

    static string CheckURL(string URL)
    {
      List<string> sb = new List<string>();
      sb.Add(URL);

      Uri uri = null;
      try { uri = new Uri(URL); }
      catch { }

      if (uri == null)
      {
        sb.Add("BAD_URI");
      }
      else
      {
        IPAddress[] ips = null;

        try { ips = Dns.GetHostAddresses(uri.DnsSafeHost); }
        catch { }

        if (ips == null)
        {
          sb.Add("DNS_FAIL");
        }
        else
        {
          sb.Add(GetHead(uri));
          sb.Add(GetString(uri));
        }
      }

      return string.Join(", ", sb.ToArray());
    }

    static string GetHead(Uri URI)
    {
      WebRequest wr = WebRequest.Create(URI);
      wr.Method = "HEAD";
      HttpWebResponse res = null;
      string msg = "";

      DateTime sTime = DateTime.Now;

      try { res = (HttpWebResponse)wr.GetResponse(); }
      catch (WebException we)
      {
        res = (HttpWebResponse)we.Response;
        msg = we.Status.ToString();
      }

      TimeSpan delta = DateTime.Now.Subtract(sTime);

      if (res != null) msg = res.StatusCode.ToString();
      return string.Format("HEAD:{0}/{1}ms", msg, delta.TotalMilliseconds);

    }

    static string GetString(Uri URI)
    {
      WebRequest wr = WebRequest.Create(URI);
      wr.Method = "GET";
      
      HttpWebResponse res = null;
      string msg = "";
      long length = 0;

      DateTime sTime = DateTime.Now;

      try { res = (HttpWebResponse)wr.GetResponse(); }
      catch (WebException we)
      {
        res = (HttpWebResponse)we.Response;
        msg = we.Status.ToString();
      }

      TimeSpan delta = DateTime.Now.Subtract(sTime);

      if (res != null) { 
        msg = res.StatusCode.ToString();
        length = res.ContentLength;
      }

      return string.Format("GET:{0}/{1}ms/{2}bytes", msg, delta.TotalMilliseconds, length);

    }

  }
}
