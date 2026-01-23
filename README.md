# MoveTo

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Windows エクスプローラーの右クリックメニューから、ファイルやフォルダを素早く指定フォルダに移動できるシェル拡張ツールです。

![Context Menu Example](docs/images/context-menu-example.png)

## ✨ 機能

- 🖱️ **右クリックメニュー統合** - エクスプローラーのコンテキストメニューに「move to」を追加
- 📁 **カスケードメニュー** - 登録済みの移動先をサブメニューで一覧表示
- 📦 **複数ファイル対応** - ファイル・フォルダを複数選択して一括移動
- ⚠️ **衝突解決** - 同名ファイル存在時に上書き/スキップ/リネーム/キャンセルを選択可能
- ⚙️ **簡単設定** - JSON設定ファイルで移動先を最大10件まで登録

## 📋 動作環境

- **OS**: Windows 10 / 11 (64-bit)
- **ランタイム**: .NET 8.0

## 🚀 インストール

### リリース版を使用する場合

1. [Releases](https://github.com/nashells/move_to/releases) から最新版をダウンロード
2. 任意の場所に展開（例: `C:\Program Files\nashells\MoveTo\`）
3. 管理者権限で PowerShell を開き、以下を実行：

```powershell
cd <展開したフォルダ>
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

4. エクスプローラーを再起動（またはWindowsを再起動）

### ソースからビルドする場合

```powershell
# リポジトリをクローン
git clone https://github.com/nashells/move_to.git
cd move_to

# ビルド
dotnet build -c Release

# リリースパッケージ作成
.\scripts\build-release.ps1
```

## ⚙️ 設定

設定ファイルの場所: `%LOCALAPPDATA%\MoveTo\config.json`

### 設定ファイルの例

```json
{
  "destinations": [
    { "displayName": "Temp", "path": "C:\\Temp" },
    { "displayName": "ダウンロード", "path": "C:\\Users\\YourName\\Downloads" },
    { "displayName": "作業フォルダ", "path": "D:\\Work" },
    { "displayName": "アーカイブ", "path": "E:\\Archive" }
  ]
}
```

### 設定項目

| プロパティ | 説明 |
|-----------|------|
| `displayName` | メニューに表示される名前 |
| `path` | 移動先フォルダの絶対パス（`\\` でエスケープ） |

> **Note**: 最大 **10件** まで登録できます。

## 📖 使い方

### 基本的な使い方

1. エクスプローラーでファイルまたはフォルダを選択
2. 右クリック → **「move to」** を選択
3. サブメニューから移動先を選択
4. ファイルが移動されます

```
右クリックメニュー          サブメニュー
┌──────────────────┐     ┌──────────────────┐
│ 開く             │     │ Temp             │
│ ─────────────── │     │ ダウンロード      │
│ move to       ▶ │ ──▶ │ 作業フォルダ      │
│ ─────────────── │     │ アーカイブ        │
│ コピー           │     └──────────────────┘
└──────────────────┘
```

### 同名ファイルが存在する場合

移動先に同じ名前のファイルがある場合、確認ダイアログが表示されます：

| オプション | 動作 |
|-----------|------|
| **上書き** | 既存ファイルを置き換える |
| **スキップ** | このファイルの移動をスキップ |
| **リネーム** | 新しい名前を付けて移動 |
| **キャンセル** | 移動処理を中止 |

### エラー時の動作

- **移動先フォルダが存在しない場合**: エラーダイアログを表示
- **アクセス権限がない場合**: エラーダイアログを表示

## 🗑️ アンインストール

管理者権限の PowerShell で以下を実行：

```powershell
cd <インストールフォルダ>
powershell -ExecutionPolicy Bypass -File .\uninstall.ps1 -RemoveFiles
```

設定ファイルも削除する場合：

```powershell
powershell -ExecutionPolicy Bypass -File .\uninstall.ps1 -RemoveFiles -RemoveConfig
```

## 🏗️ プロジェクト構成

```
move_to/
├── src/
│   ├── MoveTo.Core/        # コアロジック（設定、移動、衝突解決）
│   └── MoveTo.Shell/       # Windows シェル拡張
├── tests/
│   └── MoveTo.Core.Tests/  # ユニットテスト
├── scripts/                # ビルド・インストールスクリプト
├── docs/                   # ドキュメント
└── Release/                # リリースパッケージ
```

## 🧪 開発

### 前提条件

- Visual Studio 2022 または VS Code
- .NET 8.0 SDK
- Windows 10/11

### テスト実行

```powershell
dotnet test
```

### デバッグ

開発時は `docs/development_environment.md` を参照してください。

## 📄 技術仕様

- **シェル登録**: COM ベースのシェル拡張
- **ProgID**: `Nashells.MoveTo.ContextMenu`
- **GUID**: `{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}`
- **登録方法**: regasm を使用

## 📝 ライセンス

MIT License

## 🤝 コントリビュート

Issue や Pull Request は歓迎します！

1. Fork する
2. Feature ブランチを作成 (`git checkout -b feature/amazing-feature`)
3. 変更をコミット (`git commit -m 'Add amazing feature'`)
4. ブランチをプッシュ (`git push origin feature/amazing-feature`)
5. Pull Request を作成
