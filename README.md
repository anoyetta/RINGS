# <a href="https://github.com/anoyetta/RINGS"><img src="https://github.com/anoyetta/RINGS/blob/master/images/RINGS.Banner.png?raw=true" alt="RINGS Banner" title="RINGS" width="500" /></a>
[![Downloads](https://img.shields.io/github/downloads/anoyetta/RINGS/total.svg)](https://github.com/anoyetta/RINGS/releases)
[![License](https://img.shields.io/github/license/anoyetta/RINGS.svg)](https://github.com/anoyetta/RINGS)  

Chat communication enhancer for FFXIV.  
Discord Botと連携して、ゲーム内チャット機能を拡張するアプリケーションです。

## できること
* ゲーム内の任意のチャットチャンネルをDiscordに転送できます
* Discord上のテキストチャットをカスタマイズ可能なオーバーレイ上に表示できます
* つまり、サーバ内のローカルなLSを、クロスワールドLSのように使うことができます

### その他便利機能
* Discordテキストチャンネルに添付された画像ファイルをオーバーレイ上に表示できます
* ゲーム内チャットウィンドウのように、タブを作成して表示項目をカスタマイズできます
* キャラクターごとのプロファイル切り替えに対応しています

## 最新リリース
**[Lastest-Releases](https://github.com/anoyetta/RINGS/releases)**

## インストール
インストールは不要です。
ダウンロードしたファイルを任意のディレクトリに展開して利用してください。

RINGSはスタンドアロンで動作するため、他のアプリケーションと併用する必要はありません。

## 初期設定（一般ユーザ向け）
**Discordサーバ管理者は「Discordサーバ管理者向け設定」を併せてお読み下さい。**

### Discord Bot の設定
1. RINGS.exeを起動します。

2. "DISCORD Bot" 画面にて、Bot の設定をします。
    * **Bot名** : Botの識別子です。どのサーバか分かるよう任意の名前を入力します
    * **Token** : サーバ管理者から共有されたBotのトークンを入力します

3. "DISCORD Bot" 画面にて、DISCORD Channels の設定をします。
    * **チャンネル名** : 発言先Discordチャンネルを識別する任意の名前
    * **ID** : サーバ管理者から共有された発言先DiscordチャンネルのID
    * **Helper Bot** : 上記2.で入力した、発言先Discordチャンネルへの権限を持つBotをプルダウンで選択します

### チャットリンクの設定
"チャットリンク" 画面にて、キャラクター別プロファイルを定義します。

* キャラクター名 : あなたのキャラクター名を入力します。
* エイリアス : プレイヤーを一意に識別できるエイリアスを指定します。
    * 例えば、キャラクター名が *Rings Ffxiv*、エイリアスを *りんぐす* と設定した場合、チャット画面上には *Rings Ffxiv (りんぐす)* と表示されます。
    * エイリアスは、1人が複数のキャラクターを利用するようなケースにおいて、発言したプレイヤーを一意に識別するのに有用です。

* チャンネルリンクの設定
    * オンにしたリンクシェルに宛てたあなたの発言が、RINGSによってDiscordに転送されます。
    * 転送先となるチャンネルをプルダウンで選択できます。

複数のキャラクターがいる場合には、画面下部の + ボタンでプロファイルを複数作成することができます。

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
    * PUBLIC BOT : OFF を選択
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

#### RINGS専用チャンネルの共有メッセージの例
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

## どうやってるの？  
![RINGS_Overview1](https://github.com/anoyetta/RINGS/blob/master/images/RINGS_Overview1.png?raw=true)  
![RINGS_Overview2](https://github.com/anoyetta/RINGS/blob/master/images/RINGS_Overview2.png?raw=true)  

## トラブルシューティング
### RINGSアプリケーション設定
#### TEST CONNECTIONに失敗する
Botのトークン文字列が誤っている可能性があります。

#### チャンネルへのpingに失敗する
Botが対象Discordチャンネルのあるサーバ内に存在しないか、適切な権限がない可能性があります。

#### pingは成功するが、実際のゲーム内チャットがDiscordに送信されない
"ホーム" 画面を開き、*Acrive Profile* が inactive でないこと、および *DISCORD Bot* の状態が Ready であることを確認してください。

RINGSの設定変更を行った直後など、ゲームの監視状態をリセットするには、*RESET SUBSCRIBERS* ボタンをクリックしてください。
