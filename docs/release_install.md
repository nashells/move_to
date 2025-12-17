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
- 成果物は `src\MoveTo.Shell\bin\Release\net8.0-windows\` に配置される（MoveTo.Shell.dll, MoveTo.Core.dll, SharpShell 依存 DLL）。

## 2. インストール（管理者 PowerShell）
```powershell
Set-Location <repo_root>
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1 -RestartExplorer
```
- 既定動作: Release 出力から DLL をコピー→ `C:\Program Files\nashells\MoveTo\` へ配置→ regasm /codebase で登録→ config が無ければテンプレ生成→ Explorer を再起動。
- オプション
  - `-SkipCopy` ビルド済み配置をそのまま使う
  - `-InstallDir <path>` 配置先変更
  - `-RegasmPath <path>` regasm のパス指定
  - `-SourceDir <path>` コピー元変更

## 3. アンインストール（管理者 PowerShell）
```powershell
Set-Location <repo_root>
powershell -ExecutionPolicy Bypass -File .\scripts\uninstall.ps1 -RestartExplorer
```
- 解除のみ実施。`-RemoveFiles` を付けると配置フォルダーも削除。

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
