# kokoni-PraiseLineBot
Azure Functionsで動作するLine向けのBotになります。無条件で誉め続けるBotです。

## カスタムデプロイ

ARMテンプレートによるカスタムデプロイリンクで環境構築できるようにしました。
下記のリンクをクリックしてデプロイ環境を構築してみてください。

|デプロイ方法|デプロイリンク|
| --------------- |:---------------:|
| Direct Portal Deploy | [![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fgithub.com%2Fkingkino%2Fkokoni-PraiseLineBot%2Fblob%2Fmaster%2FAzureDeploy.json) |

## 環境構築

環境構築は下記を参照してください。
https://github.com/kingkino/kokoni-SampleLineEchoBot

※ 褒め言葉データである「data.csv」をAzureStorageに配置してください。[data]というコンテナを作成してその中に配置します。
