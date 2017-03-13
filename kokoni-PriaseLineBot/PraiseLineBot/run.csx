#r "Newtonsoft.Json"
#r "System.Configuration"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

/// <summary>
/// メインメソッド
/// </summary>
/// <param name="req"></param>
/// <param name="log"></param>
/// <returns></returns>
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Start");

    // リクエストJSONをパース
    string jsonContent = await req.Content.ReadAsStringAsync();
    Request data = JsonConvert.DeserializeObject<Request>(jsonContent);

    string replyToken = null;
    string messageType = null;

    // WebAppsのプロパティ設定からデータを取得
    var ChannelAccessToken = ConfigurationManager.AppSettings["ChannelAccessToken"];

    // リクエストデータからデータを取得
    foreach (var item in data.events)
    {
        // リプライデータ送付時の認証トークンを取得
        replyToken = item.replyToken.ToString();
        if (item.message != null)
        {
            // メッセージタイプを取得、テキスト以外はエラー文言を返却
            messageType = item.message.type.ToString();
        }
    }

    // お褒めの言葉をランダム取得
    var praiseWord = GetPraiseWordFromCSV();

    // リプライデータの作成
    var content = CreateResponse(replyToken, praiseWord, log, messageType);

    // JSON形式に変換
    var reqData = JsonConvert.SerializeObject(content);

    // レスポンスの作成
    using (var client = new HttpClient())
    {
        // リクエストデータを作成
        // ※HttpClientで[application/json]をHTTPヘッダに追加するときは下記のコーディングじゃないとエラーになる
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/v2/bot/message/reply");
        request.Content = new StringContent(reqData, Encoding.UTF8, "application/json");

        //　認証ヘッダーを追加
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ChannelAccessToken}");

        // 非同期でPOST
        var res = await client.SendAsync(request);

        return req.CreateResponse(res.StatusCode);
    }
}


/// <summary>
/// 静的データから誉め言葉をランダム取得
/// 静的データはAzureStorageに配置してあるCSV
/// </summary>
/// <returns>string</returns>
static string GetPraiseWordFromCSV()
{
    string word = "凄すぎて、誉める言葉がみあたらない！！";

    // ストレージアクセス情報の作成
    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageAccount"]);
    var blobClient = storageAccount.CreateCloudBlobClient();
    var container = blobClient.GetContainerReference(ConfigurationManager.AppSettings["StorageContainerName"]);

    // ストレージアクセスポリシーの設定
    container.SetPermissions(
        new BlobContainerPermissions
        {
            PublicAccess = BlobContainerPublicAccessType.Off,
        });

    // ブロックブロブの指定
    var blockBlob = container.GetBlockBlobReference("data.csv");

    // メモリストリームでCSVを読んで改行文字でスプリットしてランダムで誉め言葉を１件取得する
    using (var memoryStream = new MemoryStream())
    {
        blockBlob.DownloadToStream(memoryStream);
        var text = Encoding.UTF8.GetString(memoryStream.ToArray()).Split(new string[] { "\r\n" }, StringSplitOptions.None);
        var r = new Random(Environment.TickCount);
        word = text[r.Next(text.Count())];
    }

    return word;
}

/// <summary>
/// リプライ情報の作成
/// </summary>
/// <param name="token"></param>
/// <param name="praiseWord"></param>
/// <param name="log"></param>
/// <param name="messageType"></param>
/// <returns></returns>
static Response CreateResponse(string token, string praiseWord, TraceWriter log, string messageType = "")
{
    Response res = new Response();
    Messages msg = new Messages();

    // リプライトークンはリクエストに含まれるリプライトークンを使う
    res.replyToken = token;
    res.messages = new List<Messages>();

    // メッセージタイプがtext以外は単一のレスポンス情報とする
    if (messageType == "text")
    {
        msg.type = "text";
        msg.text = praiseWord;
        res.messages.Add(msg);

    }
    else
    {
        msg.type = "text";
        msg.text = "画像や動画を見せられても・・・。褒める事が出来るのはあなたの事だけです。";
        res.messages.Add(msg);
    }

    return res;
}


//　リクエスト
public class Request
{
    public List<Event> events { get; set; }
}

//　イベント
public class Event
{
    public string replyToken { get; set; }
    public string type { get; set; }
    public object timestamp { get; set; }
    public Source source { get; set; }
    public message message { get; set; }
}

// ソース
public class Source
{
    public string type { get; set; }
    public string userId { get; set; }
}

// リクエストメッセージ
public class message
{
    public string id { get; set; }
    public string type { get; set; }
    public string text { get; set; }
}


// レスポンス
public class Response
{
    public string replyToken { get; set; }
    public List<Messages> messages { get; set; }
}

// レスポンスメッセージ
public class Messages
{
    public string type { get; set; }
    public string text { get; set; }
}