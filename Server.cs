using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using EP.Ner;
using EP.Morph;

class Log
{
    public static void Info(string format, params object[] args)
    {
	Output("INFO", format, args);
    }

    public static void Error(string format, params object[] args)
    {
	Output("ERROR", format, args);
    }

    private static void Output(string type, string format, params object[] args)
    {
	string message = String.Format(format, args);
	string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
	message = String.Format("{0} [{1}] {2}", timestamp, type, message);
	Console.WriteLine(message);
    }
}

public class ConfException : Exception
{
    public ConfException(string message)
	: base(message)
    {
    }
}

class Conf
{
    public string host = "localhost";
    public int port = 8080;
    public string[] langs = {"ru", "en"};
    public string[] analyzers = {"geo", "org", "person"};

    public void Load()
    {
	var xml = LoadXml();
	ParseListen(xml);
	ParseLangs(xml);
	ParseAnalyzers(xml);
    }

    private XmlNode LoadXml()
    {
	XmlDocument xml = new XmlDocument();
	try
	{
	    xml.Load("conf.xml");
	}
	catch (FileNotFoundException)
	{
	    throw new ConfException("Missing conf.xml");
	}
	return xml.DocumentElement;
    }

    private void ParseListen(XmlNode xml)
    {
	var section = xml.SelectSingleNode("listen");
	if (section == null)
	{
	    throw new ConfException("Missing conf.listen");
	}

	var node = section.SelectSingleNode("host");
	if (node == null)
	{
	    throw new ConfException("Missing conf.listen.host");
	}
	this.host = node.InnerText;

	node = section.SelectSingleNode("port");
	if (node == null)
	{
	    throw new ConfException("Missing conf.listen.port");
	}
	string port = node.InnerText;
	if (!Int32.TryParse(port, out this.port))
	{
	    throw new ConfException(String.Format("Bad port: {0}", port));
	}
    }

    private void ParseLangs(XmlNode xml)
    {
	var section = xml.SelectSingleNode("langs");
	if (section == null)
	{
	    throw new ConfException("Missing conf.langs");
	}
	List<string> langs = new List<string>();
	XmlNodeList nodes = section.SelectNodes("lang");
	foreach (XmlNode node in nodes)
	{
	    string lang = node.InnerText;
	    if (!Pullenti.LANGS.ContainsKey(lang))
	    {
		throw new ConfException(String.Format("Bad lang: {0}", lang));
	    }
	    langs.Add(lang);
	}
	if (langs.Count == 0)
	{
	    throw new ConfException("Empty conf.langs");
	}
	this.langs = langs.ToArray();
    }

    private void ParseAnalyzers(XmlNode xml)
    {
	var section = xml.SelectSingleNode("analyzers");
	if (section == null)
	{
	    throw new ConfException("Missing conf.analyzers");
	}
	List<string> analyzers = new List<string>();
	XmlNodeList nodes = section.SelectNodes("analyzer");
	foreach (XmlNode node in nodes)
	{
	    string analyzer = node.InnerText;
	    if (!Pullenti.ANALYZERS.ContainsKey(analyzer))
	    {
		throw new ConfException(String.Format("Bad analyzer: {0}", analyzer));
	    }
	    analyzers.Add(analyzer);
	}
	if (analyzers.Count == 0)
	{
	    throw new ConfException("Empty conf.analyzers");
	}
	this.analyzers = analyzers.ToArray();
    }
}

class Pullenti
{
    public static Dictionary<string, MorphLang> LANGS = new Dictionary<string, MorphLang>
	{
	    {"ru", MorphLang.RU},
	    {"ua", MorphLang.UA},
	    {"by", MorphLang.BY},
	    {"en", MorphLang.EN},
	    {"it", MorphLang.IT},
	    {"kz", MorphLang.KZ},
	};

    public static Dictionary<string, Action> ANALYZERS = new Dictionary<string, Action>
	{
	    {"money", EP.Ner.Money.MoneyAnalyzer.Initialize},
	    {"uri", EP.Ner.Uri.UriAnalyzer.Initialize},
	    {"phone", EP.Ner.Phone.PhoneAnalyzer.Initialize},
	    {"date", EP.Ner.Date.DateAnalyzer.Initialize},
	    {"keyword", EP.Ner.Keyword.KeywordAnalyzer.Initialize},
	    {"definition", EP.Ner.Definition.DefinitionAnalyzer.Initialize},
	    {"denomination", EP.Ner.Denomination.DenominationAnalyzer.Initialize},
	    {"measure", EP.Ner.Measure.MeasureAnalyzer.Initialize},
	    {"bank", EP.Ner.Bank.BankAnalyzer.Initialize},
	    {"geo", EP.Ner.Geo.GeoAnalyzer.Initialize},
	    {"address", EP.Ner.Address.AddressAnalyzer.Initialize},
	    {"org", EP.Ner.Org.OrganizationAnalyzer.Initialize},
	    {"person", EP.Ner.Person.PersonAnalyzer.Initialize},
	    {"mail", EP.Ner.Mail.MailAnalyzer.Initialize},
	    {"transport", EP.Ner.Transport.TransportAnalyzer.Initialize},
	    {"decree", EP.Ner.Decree.DecreeAnalyzer.Initialize},
	    {"instrument", EP.Ner.Instrument.InstrumentAnalyzer.Initialize},
	    {"titlepage", EP.Ner.Titlepage.TitlePageAnalyzer.Initialize},
	    {"booklink", EP.Ner.Booklink.BookLinkAnalyzer.Initialize},
	    {"business", EP.Ner.Business.BusinessAnalyzer.Initialize},
	    {"named", EP.Ner.Named.NamedEntityAnalyzer.Initialize},
	    {"weapon", EP.Ner.Weapon.WeaponAnalyzer.Initialize},
	};

    public static void Init(string[] langs, string[] analyzers)
    {
	Log.Info("Init Pullenti v{0} ...", ProcessorService.Version);
	foreach (string lang in langs)
	{
	    Log.Info("Load lang: {0}", lang);
	    Morphology.LoadLanguages(LANGS[lang]);
	}
	ProcessorService.Initialize(Morphology.LoadedLanguages);
	foreach (string analyzer in analyzers)
	{
	    Log.Info("Load analyzer: {0}", analyzer);
	    ANALYZERS[analyzer]();
	}
    }

    public static AnalysisResult Process(string text)
    {
	Stopwatch timer = new Stopwatch();
        timer.Start();

	Processor processor = ProcessorService.CreateProcessor();  // cached
	var result = processor.Process(new SourceOfAnalysis(text));

        timer.Stop();
	TimeSpan span = timer.Elapsed;
	string time = String.Format("{0}.{1:000}", span.Seconds, span.Milliseconds);
	int size = text.Length;
	int referents = result.Entities.Count;
	Log.Info("Process: {0} chars, {1}s, {2} refs", size, time, referents);
	return result;
    }

    public static void XmlSlots(Referent referent, List<Slot> slots, XmlWriter xml)
    {
	int parent = referent.GetHashCode();
	foreach (Slot slot in slots)
	{
	    xml.WriteStartElement("slot");
	    xml.WriteAttributeString("parent", parent.ToString());
	    var value = slot.Value;
	    if (value is Referent)
	    {
	       int id = value.GetHashCode();
	       xml.WriteAttributeString("referent", id.ToString());
	    }
	    else
	    {
		xml.WriteAttributeString("value", value.ToString());
	    }
	    xml.WriteAttributeString("key", slot.TypeName);
	    xml.WriteEndElement();
	}
    }

    public static void XmlReferents(List<Referent> referents, XmlWriter xml)
    {
	foreach (Referent referent in referents)
	{
	    xml.WriteStartElement("referent");
	    int id = referent.GetHashCode();
	    xml.WriteAttributeString("id", id.ToString());
	    xml.WriteAttributeString("type", referent.TypeName);
	    xml.WriteEndElement();
	    XmlSlots(referent, referent.Slots, xml);
	}
    }

    public static void XmlMatch(ReferentToken token, Token parent, XmlWriter xml)
    {
	Referent referent = token.GetReferent();
	int start = token.BeginChar;
	int stop = token.EndChar + 1;
	int id = token.GetHashCode();
	xml.WriteStartElement("match");
	xml.WriteAttributeString("id", id.ToString());
	if (parent != null)
	{
	    id = parent.GetHashCode();
	    xml.WriteAttributeString("parent", id.ToString());
	}
	id = referent.GetHashCode();
	xml.WriteAttributeString("referent", id.ToString());
	xml.WriteAttributeString("start", start.ToString());
	xml.WriteAttributeString("stop", stop.ToString());
	xml.WriteEndElement();
	XmlMatches(token.BeginToken, token.EndToken, token, xml);
    }

    public static void XmlMatches(Token token, Token stop, Token parent, XmlWriter xml)
    {
	while (token != null)
	{
	    ReferentToken referent = token as ReferentToken;
	    if (referent != null)
	    {
		XmlMatch(referent, parent, xml);
	    }
	    if (token == stop)
	    {
		break;
	    }
	    token = token.Next;
	}
    }

    public static void XmlResult(AnalysisResult result, XmlWriter xml)
    {
	xml.WriteStartElement("result");
	XmlReferents(result.Entities, xml);
	XmlMatches(result.FirstToken, null, null, xml);
	xml.WriteEndElement();
    }
}

public class ServerException : Exception
{
    public ServerException(string message)
	: base(message)
    {
    }
}

class Server: IDisposable
{
    // Copy paste from
    // https://stackoverflow.com/questions/4672010/multi-threading-with-net-httplistener
    private readonly HttpListener listener;
    private readonly Thread thread;
    private readonly ManualResetEvent stop;

    public Server()
    {
	stop = new ManualResetEvent(false);
	listener = new HttpListener();
	thread = new Thread(HandleRequests);
    }

    public void Start(string host, int port)
    {
	string prefix = String.Format("http://{0}:{1}/", host, port);
	Log.Info("Listen prefix: {0}", prefix);
	listener.Prefixes.Add(prefix);
	listener.Start();
	thread.Start();
    }

    public void Dispose()
    {
	Stop();
    }

    public void Stop()
    {
	stop.Set();
	thread.Join();
	listener.Stop();
    }

    private void HandleRequests()
    {
	while (listener.IsListening)
	{
	    var context = listener.BeginGetContext(ListenerCallback, null);
	    WaitHandle[] events = { stop, context.AsyncWaitHandle };
	    if (WaitHandle.WaitAny(events) == 0)  // if stop
	    {
		return;
	    }
	}
    }

    private static string ReadPostData(HttpListenerRequest request)
    {
	var method = request.HttpMethod;
	if (method != "POST")
	{
	    throw new ServerException(String.Format("Bad method: {0}", method));
	}
	// https://stackoverflow.com/questions/5197579/getting-form-data-from-httplistenerrequest
	if (!request.HasEntityBody)
	{
	    throw new ServerException("No data");
	}
	using (Stream body = request.InputStream)
	{
	    var encoding = request.ContentEncoding;
	    using (StreamReader reader = new StreamReader(body, encoding))
	    {
		return reader.ReadToEnd();
	    }
	}
    }

    private static XmlWriter WriteXml(Stream stream)
    {
	XmlWriterSettings settings = new XmlWriterSettings();
	settings.Encoding = Encoding.UTF8;
	settings.Indent = true;

	return XmlWriter.Create(stream, settings);
    }

    private static void WriteResult(AnalysisResult result, HttpListenerResponse response)
    {
	response.StatusCode = 200;
	Stream output = response.OutputStream;
	using (XmlWriter xml = WriteXml(output))
	{
	    Pullenti.XmlResult(result, xml);
	}
	output.Close();
    }

    private static void WriteError(ServerException error, HttpListenerResponse response)
    {
	response.StatusCode = 400;
	Stream output = response.OutputStream;
	using (XmlWriter xml = WriteXml(output))
	{
	    xml.WriteStartElement("error");
	    xml.WriteString(error.Message);
	    xml.WriteEndElement();
	}
	output.Close();
    }

    private void ListenerCallback(IAsyncResult future)
    {
	HttpListenerContext context;
	try
	{
	    context = listener.EndGetContext(future);
	}
	catch (HttpListenerException)
	{
	    // raised by listener.Stop
	    return;
	}

	if (stop.WaitOne(0, false))
	{
	    return;
	}

	try
	{
	    string text = ReadPostData(context.Request);
	    // maybe check errors in process and write
	    // maybe errors in thread in general
	    var result = Pullenti.Process(text);
	    WriteResult(result, context.Response);
	}
	catch (ServerException error)
	{
	    Log.Error("Bad request: {0}", error.Message);
	    WriteError(error, context.Response);
	}
    }
}

class Program
{
    static void Main()
    {
	Conf conf = new Conf();
	try
	{
	    conf.Load();
	}
	catch (ConfException error)
	{
	    Log.Error("Bad conf: {0}", error.Message);
	    return;
	}

	Pullenti.Init(conf.langs, conf.analyzers);
	using (Server server = new Server())
	{
	    server.Start(conf.host, conf.port);
	    // maybe better to handle ctrl-c
	    Thread.Sleep(Timeout.Infinite);
	}
    }
}
