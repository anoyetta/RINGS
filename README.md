# RINGS
A chat communication enhancer for FFXIV

* ゲーム内の任意のチャットチャンネルをDiscordに転送できます
* Discord内のテキストチャットをカスタマイズ可能なオーバーレイ上に表示可能です
* サーバ内のローカルなLSを、クロスワールドLSのように利用することができます

## 最新リリース
**[Lastest-Releases](https://github.com/anoyetta/RINGS/releases)**

## インストール
インストールは不要です。

ダウンロードしたファイルを任意のディレクトリに展開して利用してください。

## 初期設定（一般ユーザ向け）
**Discordサーバ管理者は「Discordサーバ管理者向け設定」を併せてお読み下さい。**

1. RINGS.exeを起動します。

2. "DISCORD Bot" 画面にて、Bot の設定をします。
    * **Bot名** : Botの識別子です。どのサーバか分かるよう任意の名前を入力します
    * **Token** : サーバ管理者から共有されたBotのトークンを入力します

3. "DISCORD Bot" 画面にて、DISCORD Channels の設定をします。
    * **チャンネル名** : 発言先Discordチャンネルを識別する任意の名前
    * **ID** : サーバ管理者から共有された発言先DiscordチャンネルのID
    * **Helper Bot** : 上記2.で入力した、発言先Discordチャンネルへの権限を持つBotをプルダウンで選択します

## Discordサーバ管理者向け設定
RINGSを利用するためには、Discordサーバ管理者による作業が必要になります。

### 専用テキストチャンネルの追加（推奨）
テキストチャットをやり取りする性質上、RINGS専用のテキストチャンネルを作成することを推奨します。

### Botユーザの追加
1. **[Discord Developer
Portal](https://discordapp.com/developers/applications/)** にログインし、新規アプリケーションを作成します。

2. Botを追加します。
    * USSERNAME : ここで入力した名前がテキストチャンネル上に表示されます。
    * ICON : ここでアップロードした画像がBotユーザのアイコンになります。
    * TOKEN : *Click to Reveal Token* をクリックし、表示されたトークン文字列をコピーします。**これがDiscordサーバ内のユーザに共有するトークンです。**

3. OAuth2の設定画面にて、サーバにBotを追加するためのURLを生成します。
    * SCOPES : **bot** を選択
    * BOT PERMISSIONS : **チャンネルを見る**、**メッセージの送信**を選択
    * ページ中段にあるURLをコピーします

4. ブラウザにて、3. でコピーしたURLにアクセスし、Botを参加させるサーバを選択します。
    * 選択できるのは、自分が管理権限を持つサーバのみです。

### チャンネルIDの取得
Discordサーバにて、RINGSを通じたチャット発言先のチャンネルのIDを取得します。

1. Discordアプリ設定 > テーマ > 詳細設定 > 開発者モード をオンにします。
2. 開発者モードをオンにした状態でテキストチャンネルを右クリックすると、「IDをコピー」が選択可能になります。**ここでコピーされたIDが、Discordサーバ内のユーザに共有するチャンネルIDです。**

### BotのトークンおよびチャンネルIDをユーザに共有
予め作成しておいたRINGS専用のテキストチャンネルにピン留めしておくことを推奨します。

#### 共有サンプル

> RINGSアプリによる会話用チャンネルです。右クリックメニューから、チャンネル通知設定をミュートにすることを推奨します。
>
> アプリケーションのダウンロード
> https://github.com/anoyetta/RINGS
>
> このチャンネルのID
> 000000000000000000
>
> Botトークン
> xxxxxxxxxxxxxxxxxxxxxxxx.yyyyyy.zzzzzzzzzzzzzzzzzzzzzzzzzzz
