# 開発環境セットアップガイド

## 概要
Windows 11シェル拡張「move to」の開発環境構築手順を説明します。

---

## 1. 必要なソフトウェア

### 1.1 Visual Studio Community 2026

**ダウンロード**
- URL: https://visualstudio.microsoft.com/ja/vs/community/

**インストール時のワークロード選択（2026 以降）**
- [x] 「.NET デスクトップ開発」ワークロード
  - インストーラーの「ワークロード」タブから選択します

上記ワークロードには、NuGet パッケージマネージャーや .NET プロファイリングツール、C# / Visual Basic コンパイラなど、本プロジェクトに必要な基本コンポーネントが含まれます。特別な理由がなければ、個別コンポーネントを手動で追加指定する必要はありません。

**CLI 用 .NET SDK のインストール（推奨）**
- URL: https://dotnet.microsoft.com/ja-jp/download/dotnet
- 「.NET SDK 8.0 (LTS)」以上をインストールしてください（本プロジェクトは net8.0-windows を使用）
- インストール後、PowerShell で `dotnet --version` を実行し、SDK が認識されていることを確認します

### 1.2 Git

**ダウンロード**
- URL: https://git-scm.com/download/win

**インストールオプション（推奨）**
- Default editor: Visual Studio Code（または任意）
- PATH environment: Git from the command line and also from 3rd-party software
- Line ending: Checkout Windows-style, commit Unix-style line endings

### 1.3 VS Code（オプション、軽量編集用）

**ダウンロード**
- URL: https://code.visualstudio.com/

**推奨拡張機能**
- C# Dev Kit
- .NET Install Tool
- GitLens

---

## 2. プロジェクト構成

```
c:\Users\nashe\src\move_to\
├── docs/                              # 設計ドキュメント
├── src/
│   ├── MoveTo.Core/                   # ドメインモデル・ビジネスロジック
│   │   ├── Domain/
│   │   │   ├── Configuration/         # 設定管理
│   │   │   ├── FileMover/             # ファイル移動
│   │   │   └── ConflictResolver/      # 競合解決
│   │   └── Ports/                     # インターフェース定義
│   ├── MoveTo.Infrastructure/         # インフラ層
│   │   ├── FileSystem/                # ファイルシステム操作
│   │   ├── Configuration/             # 設定ファイル読み込み
│   │   └── Dialogs/                   # WPFダイアログ実装
│   └── MoveTo.Shell/                  # シェル拡張
│       └── ContextMenu/               # コンテキストメニュー
├── tests/
│   ├── MoveTo.Core.Tests/
│   ├── MoveTo.Infrastructure.Tests/
│   └── MoveTo.Shell.Tests/
└── MoveTo.sln
```

---

## 3. プロジェクト作成手順

### 3.1 ソリューション作成

```powershell
# ソリューションディレクトリに移動
cd c:\Users\nashe\src\move_to

# ソリューション作成
dotnet new sln -n MoveTo

# プロジェクト作成
dotnet new classlib -n MoveTo.Core -o src/MoveTo.Core -f net8.0
dotnet new classlib -n MoveTo.Infrastructure -o src/MoveTo.Infrastructure -f net8.0
dotnet new classlib -n MoveTo.Shell -o src/MoveTo.Shell -f net8.0

# テストプロジェクト作成
dotnet new xunit -n MoveTo.Core.Tests -o tests/MoveTo.Core.Tests -f net8.0
dotnet new xunit -n MoveTo.Infrastructure.Tests -o tests/MoveTo.Infrastructure.Tests -f net8.0
dotnet new xunit -n MoveTo.Shell.Tests -o tests/MoveTo.Shell.Tests -f net8.0

# ソリューションにプロジェクトを追加
dotnet sln add src/MoveTo.Core/MoveTo.Core.csproj
dotnet sln add src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj
dotnet sln add src/MoveTo.Shell/MoveTo.Shell.csproj
dotnet sln add tests/MoveTo.Core.Tests/MoveTo.Core.Tests.csproj
dotnet sln add tests/MoveTo.Infrastructure.Tests/MoveTo.Infrastructure.Tests.csproj
dotnet sln add tests/MoveTo.Shell.Tests/MoveTo.Shell.Tests.csproj
```

### 3.2 プロジェクト参照設定

```powershell
# Infrastructure -> Core
dotnet add src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj reference src/MoveTo.Core/MoveTo.Core.csproj

# Shell -> Core, Infrastructure
dotnet add src/MoveTo.Shell/MoveTo.Shell.csproj reference src/MoveTo.Core/MoveTo.Core.csproj
dotnet add src/MoveTo.Shell/MoveTo.Shell.csproj reference src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj

# テストプロジェクトの参照
dotnet add tests/MoveTo.Core.Tests/MoveTo.Core.Tests.csproj reference src/MoveTo.Core/MoveTo.Core.csproj
dotnet add tests/MoveTo.Infrastructure.Tests/MoveTo.Infrastructure.Tests.csproj reference src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj
dotnet add tests/MoveTo.Shell.Tests/MoveTo.Shell.Tests.csproj reference src/MoveTo.Shell/MoveTo.Shell.csproj
```

### 3.3 NuGetパッケージ追加

```powershell
# シェル拡張ライブラリ
dotnet add src/MoveTo.Shell/MoveTo.Shell.csproj package SharpShell

# WPF対応（Infrastructureプロジェクト）
# ※ .csprojファイルを手動編集してWPF対応にする必要あり

# テスト関連
dotnet add tests/MoveTo.Core.Tests/MoveTo.Core.Tests.csproj package Moq
dotnet add tests/MoveTo.Core.Tests/MoveTo.Core.Tests.csproj package coverlet.collector
dotnet add tests/MoveTo.Infrastructure.Tests/MoveTo.Infrastructure.Tests.csproj package Moq
dotnet add tests/MoveTo.Infrastructure.Tests/MoveTo.Infrastructure.Tests.csproj package coverlet.collector
dotnet add tests/MoveTo.Shell.Tests/MoveTo.Shell.Tests.csproj package Moq
dotnet add tests/MoveTo.Shell.Tests/MoveTo.Shell.Tests.csproj package coverlet.collector

# ロギング
dotnet add src/MoveTo.Core/MoveTo.Core.csproj package Microsoft.Extensions.Logging.Abstractions
dotnet add src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj package Microsoft.Extensions.Logging
```

---

## 4. WPFプロジェクト設定

MoveTo.Infrastructureでダイアログを使用するため、.csprojを編集してWPF対応にします。

### src/MoveTo.Infrastructure/MoveTo.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\MoveTo.Core\MoveTo.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>

</Project>
```

### src/MoveTo.Shell/MoveTo.Shell.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableComHosting>true</EnableComHosting>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\MoveTo.Core\MoveTo.Core.csproj" />
    <ProjectReference Include="..\MoveTo.Infrastructure\MoveTo.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="SharpShell" Version="2.7.2" />
  </ItemGroup>

</Project>
```

---

## 5. Git初期設定

### 5.1 リポジトリ初期化

```powershell
cd c:\Users\nashe\src\move_to
git init
```

### 5.2 .gitignore作成

```gitignore
# Build results
[Bb]in/
[Oo]bj/
[Dd]ebug/
[Rr]elease/

# Visual Studio
.vs/
*.user
*.suo
*.cache

# NuGet
packages/
*.nupkg

# Test results
TestResults/
coverage/

# OS
Thumbs.db
Desktop.ini
```

### 5.3 初回コミット

```powershell
git add .
git commit -m "Initial project setup"
```

---

## 6. シェル拡張のテスト手順

### 6.1 ビルド

```powershell
dotnet build -c Debug
```

### 6.2 シェル拡張の登録（管理者権限必要）

```powershell
# 管理者権限でPowerShellを開く
# regasmを使用して登録
$dllPath = "c:\Users\nashe\src\move_to\src\MoveTo.Shell\bin\Debug\net8.0-windows\MoveTo.Shell.dll"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /codebase $dllPath
```

### 6.3 エクスプローラー再起動

```powershell
# エクスプローラーを再起動して変更を反映
Stop-Process -Name explorer -Force
Start-Process explorer
```

### 6.4 シェル拡張の登録解除

```powershell
# 管理者権限で実行
$dllPath = "c:\Users\nashe\src\move_to\src\MoveTo.Shell\bin\Debug\net8.0-windows\MoveTo.Shell.dll"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe /unregister $dllPath
```

---

## 7. テスト実行

### 7.1 全テスト実行

```powershell
dotnet test
```

### 7.2 カバレッジ付きテスト

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### 7.3 特定プロジェクトのテスト

```powershell
dotnet test tests/MoveTo.Core.Tests/MoveTo.Core.Tests.csproj
```

---

## 8. デバッグ設定

### 8.1 シェル拡張のデバッグ

1. Visual Studioで `MoveTo.Shell` プロジェクトを右クリック
2. 「プロパティ」→「デバッグ」→「デバッグ起動プロファイルを開くUI」
3. 新しいプロファイルを作成：
   - 起動: 実行可能ファイル
   - 実行可能ファイル: `C:\Windows\explorer.exe`
4. ブレークポイントを設定してデバッグ開始

### 8.2 ダイアログのデバッグ

WPFダイアログは通常のデバッグで動作確認可能。
テスト用のコンソールアプリを作成して単独テストも可能。

---

## 9. 設定ファイルの配置

### 開発時の設定ファイルパス

```
%LOCALAPPDATA%\MoveTo\config.json
```

### config.json サンプル

```json
{
  "destinations": [
    {
      "displayName": "Temp",
      "path": "C:\\Temp"
    },
    {
      "displayName": "Documents",
      "path": "C:\\Users\\nashe\\Documents"
    }
  ]
}
```

---

## 10. トラブルシューティング

| 問題 | 解決策 |
|------|--------|
| シェル拡張が表示されない | explorer.exeを再起動、regasmの再実行 |
| 登録時にエラー | 管理者権限で実行しているか確認 |
| ビルドエラー（WPF関連） | TargetFrameworkが `net8.0-windows` になっているか確認 |
| テストが実行されない | `dotnet restore` を実行 |
| デバッグでブレークしない | 「マイコードのみ」を無効化 |

---

## 11. 参考リンク

- [SharpShell GitHub](https://github.com/dwmkerr/sharpshell)
- [.NET 8 ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/core/whats-new/dotnet-8)
- [WPF ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/desktop/wpf/)
- [xUnit ドキュメント](https://xunit.net/docs/getting-started/netcore/cmdline)
