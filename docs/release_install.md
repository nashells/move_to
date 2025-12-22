# Release ビルド・インストール・設定手順

## 前提
- OS: Windows 11
- .NET SDK: 8.0
- 配置先: `C:\Program Files\nashells\MoveTo\`
- COM 登録: `regasm.exe` (/codebase) を使用（管理者権限必須）
- 設定ファイル: `%LOCALAPPDATA%\MoveTo\config.json`

## 1. Release ビルド
```powershell
Set-Location <repo_root>
dotnet build src\MoveTo.Shell\MoveTo.Shell.csproj -c Release
```
- 成果物はシェル拡張用に `src\MoveTo.Shell\bin\Release\net48\` を使用（MoveTo.Shell.dll, MoveTo.Core.dll, SharpShell 依存 DLL）。テスト・ライブラリ向けには net8.0 もビルドされる。

## 1.5. リリースパッケージ作成
```powershell
Set-Location <repo_root>
powershell -ExecutionPolicy Bypass -File .\scripts\build-release.ps1 -Clean
```
- 成果物: `Release/` フォルダに配布用パッケージが作成される
- 含まれるもの:
  - `bin/` - ビルド済み DLL
  - `install.ps1` - インストールスクリプト
  - `uninstall.ps1` - アンインストールスクリプト
  - `README.md` - エンドユーザー向け説明書
- このフォルダを ZIP 圧縮して配布可能

## 2. インストール（管理者 PowerShell）
```powershell
Set-Location <repo_root>
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```
- 既定動作: Explorer を停止→ Release 出力から DLL をコピー→ `C:\Program Files\nashells\MoveTo\` へ配置→ regasm /codebase で COM 登録→ ContextMenuHandler をレジストリ登録→ config が無ければテンプレ生成→ Explorer を再起動。
- オプション
  - `-SkipCopy` ビルド済み配置をそのまま使う（Explorer 停止もスキップ）
  - `-InstallDir <path>` 配置先変更
  - `-RegasmPath <path>` regasm のパス指定
  - `-SourceDir <path>` コピー元変更

## 3. アンインストール（管理者 PowerShell）
```powershell
Set-Location <repo_root>
powershell -ExecutionPolicy Bypass -File .\scripts\uninstall.ps1 -RestartExplorer
```
- 解除動作: regasm /unregister で COM 登録解除→ ContextMenuHandler のレジストリ削除→ Approved リストから削除。
- オプション
  - `-RemoveFiles` 配置フォルダーも削除
  - `-RestartExplorer` Explorer を再起動

## 4. 設定ファイルの記載方法
- パス: `%LOCALAPPDATA%\MoveTo\config.json`
- 形式: JSON、最大 10 件の destinations を定義
- サンプル:
```json
{
  "destinations": [
    { "displayName": "Temp", "path": "C:\\Temp" },
    { "displayName": "Work", "path": "D:\\Work" }
  ]
}
```
- displayName: メニューに表示される名称
- path: 移動先フォルダーの絶対パス
- 注意: パス/名称が空、または 10 件超は無視される

## 5. 動作確認フロー（E2E 簡易）
1) インストール後、Explorer を右クリックして「move to」が表示されることを確認
2) 任意のファイル/フォルダーを選択し、設定した宛先へ移動できることを確認
3) 同名ファイルがある場合、上書き/スキップ/リネーム/キャンセルのダイアログが表示されることを確認
4) 移動先フォルダー不存在またはアクセス拒否時、エラーダイアログが表示されることを確認
