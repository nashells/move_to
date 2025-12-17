# Unit: インストーラー / 登録スクリプト

## 目的
MoveTo シェル拡張を配布・登録・解除するためのインストーラー（または管理者向けスクリプト）を定義する。

## 前提・前置き
- OS: Windows 11
- 配置先: `C:\Program Files\nashells\MoveTo\`
- COM 登録: regasm (.NET 4+ ツール) を /codebase で使用
- ProgID: `Nashells.MoveTo.ContextMenu`
- GUID: `{D8E8C7DA-5C4E-4B61-9A1F-4C8E9C9B7F2B}`
- 設定ファイル: `%LOCALAPPDATA%\MoveTo\config.json`（初期値を生成またはサンプルをコピー）

## 入力 / 依存物
- `MoveTo.Shell.dll`（SharpShell 依存を含むビルド成果物）
- `MoveTo.Core.dll`
- config テンプレート（任意）
- regasm パス: `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe` を想定

## 成果物 / 出力
- 管理者 PowerShell スクリプト（例: `install.ps1`, `uninstall.ps1`）
  - DLL 配置 (コピー) と regasm 登録/解除
  - ログ出力（標準出力またはファイル）
  - Explorer 再起動の案内（オプションで自動実行）
- ドキュメント: 手動実行手順の記載（deployment.md に統合）

## 実装（リポジトリ配置）
- `scripts/install.ps1`
  - 既定: `src\MoveTo.Shell\bin\Release\net8.0-windows` から DLL をコピーし、`C:\Program Files\nashells\MoveTo\` に配置
  - regasm `/codebase` で登録
  - `%LOCALAPPDATA%\MoveTo\config.json` を未作成時にテンプレ生成
  - `-RestartExplorer` で Explorer 再起動を自動実行
- `scripts/uninstall.ps1`
  - regasm `/unregister`
  - `-RemoveFiles` で配置フォルダー削除（任意）
  - `-RestartExplorer` で Explorer 再起動

## ユースケース
1. 管理者がスクリプトを実行し、MoveTo シェル拡張を登録する。
2. バージョンアップ時: 解除→ファイル差し替え→再登録。
3. アンインストール時: 解除のみ実施し、必要に応じて配置フォルダーを削除。

## 処理フロー（スクリプト例）
- install
  1. `C:\Program Files\nashells\MoveTo\` を作成
  2. DLL をコピー（`MoveTo.Shell.dll`, `MoveTo.Core.dll`, SharpShell 依存）
  3. regasm `/codebase` で登録
  4. `%LOCALAPPDATA%\MoveTo\config.json` が無ければテンプレ生成
  5. 必要に応じ Explorer を再起動
- uninstall
  1. regasm `/unregister`
  2. 必要なら配置フォルダーを削除

## 例: PowerShell スニペット
- 登録
  ```powershell
  $dll = "C:\\Program Files\\nashells\\MoveTo\\MoveTo.Shell.dll"
  & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /codebase $dll
  ```
- 解除
  ```powershell
  $dll = "C:\\Program Files\\nashells\\MoveTo\\MoveTo.Shell.dll"
  & "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\regasm.exe" /unregister $dll
  ```

## スクリプト利用例（管理者 PowerShell）
- インストール（コピー + 登録 + config 初期化 + Explorer 再起動）
  ```powershell
  Set-Location <repo_root>
  powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1 -RestartExplorer
  ```
- アンインストール（解除のみ）
  ```powershell
  Set-Location <repo_root>
  powershell -ExecutionPolicy Bypass -File .\scripts\uninstall.ps1 -RestartExplorer
  ```
- アンインストール（解除 + 配置削除）
  ```powershell
  Set-Location <repo_root>
  powershell -ExecutionPolicy Bypass -File .\scripts\uninstall.ps1 -RemoveFiles -RestartExplorer
  ```

## エラー/リカバリ
- regasm 失敗: 管理者権限/パス/依存 DLL を確認し、ログに残す。
- Explorer がメニューを更新しない: 再起動を案内または自動実行。

## 完了条件
- install/uninstall スクリプトがリポジトリに含まれ、手順が deployment.md に反映されている。
- スクリプト実行のみで登録と解除が可能であることを確認済み（E2E の一部）。
